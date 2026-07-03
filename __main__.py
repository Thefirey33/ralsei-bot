"""
Ralsei, The fluffy bodyguard.
This is the bot-code for Ralsei, the discord bot that will manage the Thefirey33 discord server.
"""

from discord.ext.commands import Bot, Cog
from discord.abc import GuildChannel
from discord.ext.tasks import loop
from dotenv import load_dotenv
from typing import Literal
import discord
import logging
import datetime
import asyncio
import random
import os

# General constants
ONE_HOUR_SECONDS = 3600
# Random qoute checking
RANDOM_START = 40
RANDOM_END = 120
MINIMUM_LAST_MESSAGE_HOUR = 5
LOG_OUTPUT = 120
# Other general constants
ONE_YEAR = 365
DEFAULT_TASK_KEYWORD = "pls"
WAIT_TIME = 10

# Logging functions
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger("ralsei")
load_dotenv()

# Execute specified tasks at certain times,
# Make the bot enter hibernation mode etc.
default_timezone = datetime.timezone(datetime.timedelta(hours=3))
beginning_time = datetime.time(hour=10, tzinfo=default_timezone)
ending_time = datetime.time(hour=22, tzinfo=default_timezone)

# Ralsei qoutes that the bot will randomly send.
random_qoutes_file = open("ralsei_qoutes.txt", "r")
random_qoutes = random_qoutes_file.readlines()
logger.info(f"Imported random qoutes as {random_qoutes}")

class RalseiRandomQoutesCheckCog(Cog):
    """
    The whole purpose of this cog is to keep the server alive by making the bot send random qoutes in the general chat.
    Every specified number of hours, the bot will check if a message hasn't been sent for a long time.
    If a message hasn't been sent for a long time, the bot will send a message with a random qoute/message.
    !!! The bot WILL NOT send a message if it's the last message sender.
    !!! The bot WILL NOT send a message, if it's latest message isn't at least the specified number of messages away.
    !!! The bot WILL NOT send a message, if it's late in the GMT + 3 timezone. If it is, the bot will show the IDLE status and will not send messages!
    """

    def pick_random_time(self):
        return random.randrange(RANDOM_START, RANDOM_END) * ONE_HOUR_SECONDS

    def __init__(self, bot: RalseiBot):
        super().__init__()
        self.bot = bot
        # Pick a random time to start off with.
        self.time_seconds_left = self.pick_random_time()
        # Start the general ticker.
        self.ticking.start()
        
    def cog_unload(self):
        # Stop the general ticker.
        self.ticking.cancel()
        # Unload the cog.
        return super().cog_unload()
    
    async def message_sending_check(self, channel: discord.TextChannel):
        
        last_message_in_channel = [message async for message in channel.history(limit=1)]
        last_message = last_message_in_channel[0]
        
        # Check if the bot sent the latest message.
        if last_message.author == self.bot.user:
            logger.info("Last message was sent by the bot, ignoring..")
            return False
        
        # If the ralsei bot is not active, do not allow a message to be sent.
        if not self.bot.is_interaction_active:
           return False
        
        # Check if the latest message was sent a long time ago.
        timespan = datetime.datetime.now().replace(tzinfo=default_timezone) - last_message.created_at.replace(tzinfo=default_timezone)
        if timespan.seconds < ONE_HOUR_SECONDS * MINIMUM_LAST_MESSAGE_HOUR:
            logger.info("Last message is recent, ignoring...")
            return False
        
        # Check if the bot has already send a message, if it is, return False.
        async for message in channel.history(limit=10):
            if message.author.id == self.bot.user.id:
                logger.info("Bot has already send a message before, ignoring...")
                return False
            
        return True
        
    
    @loop(seconds=1)
    async def ticking(self):
        self.time_seconds_left -= 1
        # Show the current state to the console.
        if self.time_seconds_left % LOG_OUTPUT == 0:
            logger.info(f"Next qoute in {datetime.timedelta(seconds=self.time_seconds_left)}!")
        # If below zero, send a random qoute.
        channel = await self.bot.get_logging_channel()
        
        if self.time_seconds_left < 0:
            # Enact a check that must do the specified steps.
            if not await self.message_sending_check(channel):
                self.time_seconds_left = self.pick_random_time()
                return
            
            await channel.send(random.choice(random_qoutes))
            self.time_seconds_left = self.pick_random_time()

class RalseiHibernationCheckCog(Cog):
    """
    This is the hibernation checker cog.
    This allows the Ralsei bot to not operate during night time and leave the server unmoderated without Thefirey33's supervision.
    """
    
    
    async def set_hibernation_state(self):
        current_time = datetime.datetime.now(default_timezone)
        logger.info("Updating bot state...")
        
        # If the current time is over the ending time, set it to IDLE mode.
        # This is when the bot will not send any messages.
        if current_time.hour > ending_time.hour or current_time.hour < beginning_time.hour:
            await self.bot.change_presence(status=discord.Status.idle, activity=discord.Game("*fluffy boy is sleeping*"))
            self.bot.is_interaction_active = False
            return
        
        # If not, set the bot to online mode again, and the interactions will resume.
        if current_time.hour > beginning_time.hour:
            await self.bot.change_presence(status=discord.Status.online, activity=discord.Game("*hanging around in Castle Town*"))
            self.bot.is_interaction_active = True
    
    def __init__(self, bot: RalseiBot):
        super().__init__()
        self.bot = bot
        # Start the hibernation times counter.
        self.hibernation_times.start()
        logger.info("The hibernation checking cog is active!")
        
    def cog_unload(self):
        # Unload the hibernation task.
        self.hibernation_times.cancel()
        return super().cog_unload()
        
    
    @loop(time=[beginning_time, ending_time])
    async def hibernation_times(self):
        await self.set_hibernation_state()


class RalseiBot(Bot):
    """
    The Ralsei Bot's existence is in this class.
    """
        
    def __init__(self):
        super().__init__(command_prefix=DEFAULT_TASK_KEYWORD, intents=discord.Intents.all())
        # Is the interaction active.
        # Basically, should the bot start saying random qoutes?
        self.is_interaction_active = False
    
    async def on_ready(self):

        logger.info("Adding cogs")
        hibernation_check_cog = RalseiHibernationCheckCog(self)
        await hibernation_check_cog.set_hibernation_state()
        await self.add_cog(hibernation_check_cog)
        await self.add_cog(RalseiRandomQoutesCheckCog(self))
        # After all the cogs are added, move onto syncing
        logger.info("Cog initialization is done, Bot is active")
    
    async def log_something(self, message: str):
        logging_channel = await self.get_logging_channel()
        logging_channel.send(message)
    
    async def get_moderation_channel(self) -> discord.TextChannel:
        return await self.fetch_channel(os.environ["MODERATION_CHANNEL"])
    
    async def get_general_channel(self) -> discord.TextChannel:
        return await self.fetch_channel(os.environ["GENERAL_CHANNEL"])
    
    async def get_logging_channel(self) -> discord.TextChannel:
        return await self.fetch_channel(os.environ["LOGGING_CHANNEL"])
    
    async def kick_user(self, member: discord.Member, kick_reason: str):
        """Kicks a member.

        Args:
            member (discord.Member): The member to kick.
            kick_reason (str): The kicking reason.
        """
        logger.info(f"Kicking member with id: {member.id}, reason: {kick_reason}")
        await member.kick(reason=kick_reason)
        
        # Announce kick to the user-base.
        channel = await self.get_moderation_channel()
        await channel.send(f"<:ralsei_angry:1522686576608542861> {member.name} was kicked for {kick_reason}")
        
    async def ban_user(self, member: discord.Member, ban_reason: str):
        """Bans a member.

        Args:
            member (discord.Member): The member to ban.
            ban_reason (str): The reason for the ban.
        """
        
        logger.info(f"BANNING member with id: {member.id}, reason: {ban_reason}")
        await member.ban(reason=ban_reason)
        
        # Announce ban to the user-base.
        channel = await self.get_moderation_channel()
        await channel.send(f"<:ralsei_angry:1522686576608542861> {member.name} was banned for {ban_reason}")
        
    async def unban_user(self, member: discord.Member, reason: str):
        """Unbans a member.

        Args:
            member (discord.Member): The member that will be unbanned.
            reason (str): The reason for the unban.
        """
        
        logger.info(f"Unbanning member with id: {member.id}, reason: {reason}")
        await member.unban(reason=reason)
        
        # Inform the target member that they've been unbanned!
        if member.dm_channel:
            await member.dm_channel.send("<:ralsei_happy:1522702707872239646> You have been unbanned from the server!")
    
    
    async def run_member_checks(self, member: discord.Member):
        time_difference = datetime.datetime.now().replace(tzinfo=default_timezone) - member.created_at.replace(tzinfo=default_timezone)
        
        # A fail-safe in case something happens.
        day_difference = abs(time_difference.days)
        
        # Check if the user's account is too new, if it is, don't allow them to join.
        if day_difference <= ONE_YEAR:
            await self.kick_user(member, "Account Too New")
            return
            
        # If this user doesn't contain an avatar and has a default avatar, kick them immediately.
        if not member.avatar:
            await self.kick_user(member, "Account Too Suspicious")
        
    async def on_member_join(self, member: discord.Member):
        if member.bot:
            logger.info(f"Detected {member.id} is a bot, ignoring...")
            return
        
        await self.run_member_checks(member)
        await self.log_something(f"{member.name} with ID {member.id} has joined!")
    
    async def on_member_update(self, before: discord.Member, after: discord.Member):
        await self.log_something(f"{before.name} -> {after.name}\n{after.name} with id: {after.id}!")
    
    async def on_message(self, message: discord.Message):
        if message.author.id != int(os.environ["TRUSTED_USER"]) and message.author != self:
            return
        
        message_split = message.clean_content.strip().split()
        
        # Check if the trusted user has requested a task.
        # First, the message must be made out of minimum 3 words/splits, if not, the message will be ignored.
        if message_split.__len__() < 2:
            return
        
        if message_split[0] == DEFAULT_TASK_KEYWORD:
            # If Thefirey33 accidentally does a type on the ID, ignore the task.
            if not message_split[2].isnumeric():
                await message.reply("I'm sorry sir, but... That's not really an ID :c")
                logger.info("Trusted user attempted to do action, error occured so ignoring task...")
                
                return
            
            user_id = int(message_split[2])
            recieved_member = message.guild.get_member(user_id)
            
            # If the requested member does not exist in the server, 
            # Ignore the task and move onwards.
            if not recieved_member:
                await message.reply("I'm sorry sir, but... they don't exist :c")
                logger.info("Trusted user attempted to do action, error occured so ignoring task...")
                
                return
            
            # Where the real magic happens.
            if message_split.__len__() > 3:
                reason = message_split[3]
            else:
                reason = "The Moderation Hammer Has Spoken!"
            match message_split[1]:
                case "kick":
                    await self.kick_user(recieved_member, reason)
                case "ban":
                    await self.ban_user(recieved_member, reason)
                case "unban":
                    await self.unban_user(recieved_member, reason)

    
if __name__ == "__main__":
    ralsei_bot_session = RalseiBot()
    ralsei_bot_session.run(os.environ["DISCORD_TOKEN"], log_handler=None)