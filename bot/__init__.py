import asyncio
import inspect
import json
import sys
from datetime import datetime, time

import discord
from better_profanity import profanity
from discord import (
    DMChannel,
    Guild,
    Intents,
    Message,
    Status,
    TextChannel,
    VoiceChannel,
)
from discord.ext.commands import Bot, Cog
from discord.ext.tasks import loop

from bot.commands import GeneralCommands
from bot.data import RalseiDataManager
from bot.database_manager import RalseiBotDatabaseManager, RalseiBotDatabaseModal
from definitions import (
    MESSAGE_TYPE_DIVISION,
    PAT_PAT_TYPES,
    active_timezone,
    get_trusted_id,
)
from funsies import simulate_ralsei_sleep, simulate_ralsei_wake
from logsystem import ralsei_bot_logger
from security.member_security import MemberSecurityCog

# NOTE: If you remove this line, the language server will start bitching. So don't.
__all__ = ["MemberSecurityCog", "GeneralCommands"]

# This is the time defined where Ralsei 'wakes up'
beginning_time = time(hour=8, tzinfo=active_timezone)

# This is the time defined where Ralsei 'sleeps'.
ending_time = time(hour=23, tzinfo=active_timezone)

gif_sources: dict[str, str] = json.load(open("sleep_and_awake_source.json"))
ralsei_bot_logger.info("Imported gif sources as %s", gif_sources)

class RalseiActiveCog(Cog):
    """
    This allows the Ralsei bot to act more realistic.
    In the GMT + 3 timezone cycle, it allows Ralsei to go IDLE or ONLINE.
    Along with simulating that Ralsei might be watching some random shows during the day.
    """

    async def send_message_to_all_guilds(self, msg: str):
        async for guild in self.ralsei_bot.fetch_guilds():
            server_info = self.ralsei_bot.database_manager.get_server_information(guild.id)
            if not server_info:
                return

            channel = await guild.fetch_channel(server_info.ralsei_channel)
            if not isinstance(channel, TextChannel):
                return

            await self.ralsei_bot.send_message_defined(channel, msg)



    async def update_bot_presence(self, initial_send: bool = False):
        current_time = datetime.now(active_timezone)

        # This is when the Ralsei bot is sleeping.
        if (
            current_time.hour >= ending_time.hour
            or current_time.hour < beginning_time.hour
        ):
            if not initial_send:
                await self.send_message_to_all_guilds(gif_sources["sleep"])
            await self.ralsei_bot.change_presence(
                status=Status.idle, activity=discord.Game(name="*fluffy boi is asleep*")
            )
            self.ralsei_bot.awake = False
        elif current_time.hour >= beginning_time.hour:
            if not initial_send:
                await self.send_message_to_all_guilds(gif_sources["awake"])
            self.ralsei_bot.awake = True
            await self.ralsei_bot.change_presence(
                status=Status.online,
                activity=discord.Game(name="*fluffy boi is chillin'*"),
            )

        ralsei_bot_logger.info("Ralsei's awakeness state has been updated.")

    def __init__(self, ralsei_bot):
        self.ralsei_bot: RalseiBot = ralsei_bot

        # Create the specified tasks.
        asyncio.create_task(self.update_bot_presence(initial_send=True), eager_start=True)
        self.bot_hibernation_state.start()

    async def cog_unload(self):
        self.bot_hibernation_state.cancel()

    @loop(time=[beginning_time, ending_time])
    async def bot_hibernation_state(self):
        """
        This allows the Ralsei bot to have an active 'Sleep Schedule'.
        This makes it so Ralsei is awake at daytime, but sleeping at nighttime.
        :return: None
        """
        await self.update_bot_presence()


class RalseiBot(Bot):
    awake: bool
    """
    This is when Ralsei is awake and will do random actions.
    """

    def __init__(self):
        super().__init__(
            command_prefix="!", intents=Intents.all(), status=Status.offline
        )
        self.database_manager = RalseiBotDatabaseManager()
        self.data_manager = RalseiDataManager()
        profanity.load_censor_words(
            self.data_manager.get_data_by_key("serious_discussion_words")
        )

    async def check_guild(self, guild: Guild):
        ralsei_bot_logger.info("Checking guild: %s...", guild.name)
        channels = await guild.fetch_channels()
        text_channels: list = list(
            filter(lambda channel: isinstance(channel, discord.TextChannel), channels)
        )

        def search_channel(name_match: str):
            """
            Searches for a matching name in the text channel list.
            :param name_match: The name match to search for, it will search if the specified channel contains it.
            :return: The channel found, might be null.
            """
            return next(
                filter(
                    lambda channel: channel.name.__contains__(name_match), text_channels
                ),
                None,
            )

        # The specified channels that the bot will search for,
        # Since these already exist in my server, we are in the clear.
        general_channel: TextChannel | None = search_channel("general")
        moderation_channel: TextChannel | None = search_channel("moderation")
        ralsei_channel: TextChannel | None = search_channel("ralsei")

        if general_channel and moderation_channel and ralsei_channel:
            self.database_manager.add_server_to_database(
                RalseiBotDatabaseModal(
                    guild.id,
                    general_channel.id,
                    ralsei_channel.id,
                    moderation_channel.id,
                )
            )
        else:
            ralsei_bot_logger.warning(
                "Couldn't auto-configure for server with id: %s.", guild.id
            )

    async def reply_message(self, msg: str, message: Message):
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

    async def register_cog(self, cog):
        """
        Registers a cog.
        :param cog: The cog to register.
        """
        ralsei_bot_logger.info("Registering cog: %s", cog.__name__)
        await self.add_cog(cog(self))

    async def start_typing(
        self,
        msg: str,
        message_type_penalty: float,
        channel: DMChannel | VoiceChannel | TextChannel,
    ):
        # Simulate typing.
        async with channel.typing():
            await asyncio.sleep(message_type_penalty)
        await channel.send(msg)

    async def send_message(self, channel_id: int, msg: str):
        """
        Simulate Ralsei actually typing a message.
        :param action: Optional action it can take instead.
        :param channel_id: The channel to send the message to.
        :param msg: The message to send.
        """

        message_type_penalty = msg.__len__() / MESSAGE_TYPE_DIVISION
        channel = await self.fetch_channel(channel_id)
        if channel and isinstance(channel, TextChannel):
            text_channel: TextChannel = channel
            ralsei_bot_logger.info("Sending message: %s, Ralsei is typing!", msg)

            await self.start_typing(msg, message_type_penalty, text_channel)

    async def send_message_defined(
        self, channel: DMChannel | TextChannel | VoiceChannel, msg: str
    ):
        """
        This sends a message to the specified channel, but the channel argument is DEFINED.
        :param channel: The channel itself.
        :param msg: The message to send.
        """
        message_type_penalty = msg.__len__() / MESSAGE_TYPE_DIVISION

        if channel:
            ralsei_bot_logger.info("Sending message: %s, Ralsei is typing!", msg)
            await self.start_typing(msg, message_type_penalty, channel)

    async def on_ready(self):
        # Register all the cogs to the bot.
        # The Registry is automatic, so all the cogs will get registered quickly.
        for _, cog_obj in inspect.getmembers(sys.modules[__name__]):
            if inspect.isclass(cog_obj) and issubclass(cog_obj, Cog) and cog_obj != Cog:
                await self.register_cog(cog_obj)

        async for guild in self.fetch_guilds(limit=None):
            # Check if the specified server exists in the database, if it doesn't, add it to the listing.
            if not self.database_manager.check_if_server_exists(guild.id):
                await self.check_guild(guild)

            self.tree.copy_global_to(guild=guild)
            ralsei_bot_logger.info("Copied commands to guild: %s", guild.name)

        await self.tree.sync()
        ralsei_bot_logger.info("Successfully synced all commands, bot is initialized!")

    async def on_message(self, message: Message) -> None:
        if message.author.bot:
            return

        # If something that is out of line is said, Ralsei is instructed to immediately purge the message.
        # Only the serious chats will be free of this strict moderation.
        await simulate_ralsei_wake(self)

        # Profanity detection.
        content_clean = message.clean_content

        # To stop the language server from bitching.
        if not isinstance(message.channel, TextChannel) or not self.user:
            return

        channel_is_serious = message.channel.name.__contains__("serious")

        # Split the message into each word, so we can detect if the user has pat-patted ralsei.
        msg_action_split = message.clean_content.split()
        detect_action = any(
            map(lambda x: PAT_PAT_TYPES.__contains__(x), msg_action_split)
        )

        if (
            profanity.contains_profanity(content_clean)
            and not channel_is_serious  # If the channel is not the serious channel, then the delete action will be taken.
            and not message.author.id == get_trusted_id()
        ):
            reply_msg = await self.reply_message(
                self.data_manager.get_data_by_key_rand("scold"), message
            )

            # Delete the message, then move on.
            await message.delete(delay=2)
            await reply_msg.delete(delay=2)

        elif message.mentions.__contains__(self.user) and detect_action:
            await self.reply_message(
                self.data_manager.get_data_by_key_rand("pleasant_reactions"),
                message,
            )

        await simulate_ralsei_sleep(self)
