import discord
from discord.ext.commands import Cog
from discord.ext.commands.hybrid import app_commands

from bot.data import RalseiDataManager
from bot.database_manager import RalseiBotDatabaseManager
from definitions import get_trusted_id
from funsies import simulate_ralsei_sleep, simulate_ralsei_wake
from logsystem import ralsei_bot_logger
from security import ban_member, kick_member


class GeneralCommands(Cog):
    """
    All the moderation commands for the bot.
    """

    def __init__(self, bot) -> None:
        super().__init__()
        self.bot = bot
        self.db_manager: RalseiBotDatabaseManager = self.bot.database_manager
        self.data_manager: RalseiDataManager = self.bot.data_manager

        async def privilege_check(interaction: discord.Interaction):
            if interaction.user.id != get_trusted_id():
                # If the user attempts to execute the allow entry command without authorization, Ralsei will refuse to do so.
                await interaction.response.send_message(
                    self.data_manager.get_data_by_key_rand("scold")
                )
                return False
            return True

        #
        # This adds an allowed entry to the database, so a user in question can join.
        #
        @self.bot.tree.command(
            name="add_entry",
            description="Allow entry for a member. (Must be Thefirey33 to use this command.)",
        )
        @app_commands.describe(
            user_id="The User ID of the member you want to approve entry for."
        )
        async def add_entry(interaction: discord.Interaction, user_id: int):
            if not privilege_check(interaction):
                return

            # Add to the approved list.
            self.db_manager.add_allowed(user_id)
            await interaction.response.send_message(
                "<:ralsei_happy:1522702707872239646> approved entry for them!!",
                ephemeral=True,
            )

        #
        # This command kicks a member from the server.
        #
        @self.bot.tree.command(
            name="kick_member",
            description="Kick a member. (Must be Thefirey33 to use this command.)",
        )
        @app_commands.describe(member="The Member in question.", reason="The reason.")
        async def kick(
            interaction: discord.Interaction, member: discord.Member, reason: str
        ):
            if not await privilege_check(interaction):
                return

            await interaction.response.send_message(
                "okay.. i'll be right on it...", ephemeral=True
            )
            await kick_member(self.bot, self.db_manager, member, reason=reason)

        #
        # This command bans a member from the server.
        #
        @self.bot.tree.command(
            name="ban_member",
            description="Ban a member. (Must be Thefirey33 to use this command.)",
        )
        @app_commands.describe(member="The Member in question.", reason="The reason.")
        async def ban(
            interaction: discord.Interaction, member: discord.Member, reason: str
        ):
            if not await privilege_check(interaction):
                return

            await interaction.response.send_message(
                "gone forever? well... bye to them...", ephemeral=True
            )
            await ban_member(self.bot, self.db_manager, member, reason=reason)

        @self.bot.tree.command(
            name="response", description="Create a response to message."
        )
        async def response(
            interaction: discord.Interaction,
            channel_id: str,
            message_id: str,
            message: str,
        ):
            if interaction.user.id != get_trusted_id():
                # If the user attempts to execute the allow entry command without authorization, Ralsei will refuse to do so.
                await interaction.response.send_message(
                    self.data_manager.get_data_by_key_rand("scold")
                )
                return

            if not (channel_id.isnumeric() or message_id.isnumeric()):
                ralsei_bot_logger.warning("ID error, skipping response...")
                return

            if not interaction.guild:
                ralsei_bot_logger.warning("Guild doesn't exist, skipping response...")
                return

            channel = await interaction.guild.fetch_channel(int(channel_id))

            if not isinstance(channel, discord.TextChannel):
                return

            msg = await channel.fetch_message(int(message_id))
            await simulate_ralsei_wake(self.bot)
            await self.bot.reply_message(message, msg)
            await simulate_ralsei_sleep(self.bot)
