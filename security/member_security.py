from datetime import datetime, timedelta

from discord import Member
from discord.ext.commands import Cog

from bot import RalseiBotDatabaseManager
from definitions import active_timezone, ONE_YEAR
from funsies import simulate_ralsei_wake, simulate_ralsei_sleep, apply_sleep
from logsystem import ralsei_bot_logger
from security import kick_member


class MemberSecurityCog(Cog):
    """
    This allows the server's member joining to be more managed,
    The bot will check different attributes about a member.
    """

    def __init__(self, bot) -> None:
        self.bot = bot

    @Cog.listener()
    async def on_member_join(self, member: Member):
        time_diff = datetime.now().replace(tzinfo=active_timezone) - member.created_at.replace(tzinfo=active_timezone)

        db_manager: RalseiBotDatabaseManager = self.bot.database_manager

        # If the member was pre-approved for entry, then the bot will let them pass.
        if db_manager.is_member_allowed(member.id):
            ralsei_bot_logger.info("Member %d was approved for entry!", member.id)
            return

        # This checks if the specified member's discord account age is not over a year old.
        if time_diff < timedelta(days=ONE_YEAR):
            await kick_member(self.bot, db_manager, member, "account is too new")
            return

        # If the member doesn't have an avatar, which majority of discord users DO HAVE an avatar,
        # Immediately kick them.
        if not member.avatar:
            await kick_member(self.bot, db_manager, member, "Account Too Suspicious!")

        server_info = db_manager.get_server_information(member.guild.id)
        if not server_info:
            return

        # Send a message to welcome the person.
        # If they are a bot, don't welcome them because they are... you know? a bot.
        if member.bot:
            return

        await apply_sleep()
        await simulate_ralsei_wake(self.bot)
        await self.bot.send_message(server_info.general_channel,
                                    self.bot.data_manager.get_data_by_key_rand("introduction").format(
                                        member.mention))
        await simulate_ralsei_sleep(self.bot)
