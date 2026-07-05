import asyncio
import os
from datetime import time, datetime

import discord
from better_profanity import profanity
from discord import Intents, Status, TextChannel, Guild, Message
from discord.ext.commands import Bot, Cog
from discord.ext.tasks import loop

from bot.data import RalseiDataManager
from bot.database_manager import RalseiBotDatabaseManager, RalseiBotDatabaseModal
from definitions import active_timezone, MESSAGE_TYPE_DIVISION
from funsies import simulate_ralsei_wake, simulate_ralsei_sleep
from logsystem import ralsei_bot_logger
from security.member_security import MemberSecurityCog

# This is the time defined where Ralsei 'wakes up'
beginning_time = time(hour=8, tzinfo=active_timezone)

# This is the time defined where Ralsei 'sleeps'.
ending_time = time(hour=23, tzinfo=active_timezone)


class RalseiActiveCog(Cog):
    """
    This allows the Ralsei bot to act more realistic.
    In the GMT + 3 timezone cycle, it allows Ralsei to go IDLE or ONLINE.
    Along with simulating that Ralsei might be watching some random shows during the day.
    """

    async def update_bot_presence(self):
        current_time = datetime.now(active_timezone)

        # This is when the Ralsei bot is sleeping.
        if current_time.hour >= ending_time.hour or current_time.hour < beginning_time.hour:
            await self.ralsei_bot.change_presence(
                status=Status.idle,
                activity=discord.Game(name="*fluffy boi is asleep*")
            )
            self.ralsei_bot.awake = False

        if current_time.hour >= beginning_time.hour:
            self.ralsei_bot.awake = True
            await self.ralsei_bot.change_presence(
                status=Status.online,
                activity=discord.Game(name="*fluffy boi is chillin'*")
            )

        ralsei_bot_logger.info("Ralsei's awakeness state has been updated.")

    def __init__(self, ralsei_bot: RalseiBot):
        self.ralsei_bot = ralsei_bot

        # Create the specified tasks.
        asyncio.create_task(self.update_bot_presence(), eager_start=True)
        self.bot_hibernation_state.start()

    def cog_unload(self) -> None:
        self.bot_hibernation_state.cancel()

    @loop(time=[beginning_time, ending_time])
    async def bot_hibernation_state(self):
        """
        This allows the Ralsei bot to have an active 'Sleep Schedule'.
        This makes it so Ralsei is awake at daytime, but sleeping at nighttime.
        :return: None
        """
        await self.update_bot_presence()


async def reply_message(msg: str, message: Message):
    """
    Simulate Ralsei actually typing a message.
    :param message: The message to reply to.
    :param msg: The message to send.
    """

    message_type_penalty = msg.__len__() / MESSAGE_TYPE_DIVISION
    channel = message.channel
    ralsei_bot_logger.info("Sending message: %s, Ralsei is typing!", msg)

    # Simulate typing.
    async with channel.typing():
        await asyncio.sleep(message_type_penalty)
    return await message.reply(msg)


class RalseiBot(Bot):
    awake: bool
    """
    This is when Ralsei is awake and will do random actions.
    """

    def __init__(self):
        super().__init__(command_prefix="pls", intents=Intents.all(), status=Status.offline)
        self.database_manager = RalseiBotDatabaseManager()
        self.data_manager = RalseiDataManager()
        profanity.load_censor_words(self.data_manager.get_data_by_key("serious_discussion_words"))

    async def check_guild(self, guild: Guild):
        ralsei_bot_logger.info("Checking guild: %s...", guild.name)
        channels = await guild.fetch_channels()
        text_channels: list[TextChannel] = list(
            filter(lambda channel: isinstance(channel, discord.TextChannel), channels))

        def search_channel(name_match: str):
            """
            Searches for a matching name in the text channel list.
            :param name_match: The name match to search for, it will search if the specified channel contains it.
            :return: The channel found, might be null.
            """
            return next(filter(lambda channel: channel.name.__contains__(name_match), text_channels), None)

        # The specified channels that the bot will search for,
        # Since these already exist in my server, we are in the clear.
        general_channel: TextChannel | None = search_channel("general")
        moderation_channel: TextChannel | None = search_channel("moderation")
        ralsei_channel: TextChannel | None = search_channel("ralsei")

        if general_channel and moderation_channel and ralsei_channel:
            self.database_manager.add_server_to_database(
                RalseiBotDatabaseModal(guild.id, general_channel.id, ralsei_channel.id,
                                       moderation_channel.id))
        else:
            ralsei_bot_logger.warning("Couldn't auto-configure for server with id: %s.", guild.id)

    async def register_cog(self, cog):
        """
        Registers a cog.
        :param cog: The cog to register.
        """
        ralsei_bot_logger.info("Registering cog: %s", cog.__name__)
        await self.add_cog(cog(self))

    async def send_message(self, channel_id: int, msg: str):
        """
        Simulate Ralsei actually typing a message.
        :param action: Optional action it can take instead.
        :param channel_id: The channel to send the message to.
        :param msg: The message to send.
        """

        message_type_penalty = msg.__len__() / MESSAGE_TYPE_DIVISION
        channel = await self.fetch_channel(channel_id)
        if channel:
            ralsei_bot_logger.info("Sending message: %s, Ralsei is typing!", msg)

            # Simulate typing.
            async with channel.typing():
                await asyncio.sleep(message_type_penalty)
            await channel.send(msg)

    async def on_ready(self):
        await self.register_cog(RalseiActiveCog)
        await self.register_cog(MemberSecurityCog)

        async for guild in self.fetch_guilds(limit=None):
            # Check if the specified server exists in the database, if it doesn't, add it to the listing.
            if not self.database_manager.check_if_server_exists(guild.id):
                await self.check_guild(guild)

    async def on_message(self, message: Message) -> None:
        if message.author.bot or message.author.id == int(os.environ["TRUSTED_USER"]):
            return

        # If something that is out of line is said, Ralsei is instructed to immediately purge the message.
        # Only the serious chats will be free of this strict moderation.
        await simulate_ralsei_wake(self)

        # Profanity detection.
        content_clean = message.clean_content
        channel_is_serious = message.channel.name.__contains__("serious")
        
        if profanity.contains_profanity(content_clean) and not channel_is_serious:
            reply_msg = await reply_message(self.data_manager.get_data_by_key_rand("scold"), message)

            # Delete the message, then move on.
            await message.delete(delay=2)
            await reply_msg.delete(delay=2)
        elif message.mentions.__contains__(self.user) and not channel_is_serious:
            if message.content.__contains__(
                    "patpat") or message.content.__contains__("petpet"):
                await reply_message(self.data_manager.get_data_by_key_rand("pleasant_reactions"), message)
            else:
                await reply_message(self.data_manager.get_data_by_key_rand("other_introduction"), message)

        await simulate_ralsei_sleep(self)
