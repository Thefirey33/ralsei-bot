import discord

from bot import RalseiBotDatabaseManager
from funsies import simulate_ralsei_wake, simulate_ralsei_sleep
from logsystem import ralsei_bot_logger


async def general_action(action, action_type: str, ralsei_bot, database_manager: RalseiBotDatabaseManager,
                         member: discord.Member,
                         reason: str = "The Kick Hammer Has Spoken!"):
    server_info = database_manager.get_server_information(member.guild.id)
    if not server_info:
        ralsei_bot_logger.warning("Server with id %d doesn't exist in database!", member.guild.id)
        return
    await simulate_ralsei_wake(ralsei_bot)
    # Finally kick the person.
    await action(reason=reason)
    # Send message to the moderation channel.
    await ralsei_bot.send_message(server_info.moderation_channel,
                                  f"<:ralsei_sad:1523124485010489607> kicked {member.name} because their {reason}...")
    await simulate_ralsei_sleep(ralsei_bot)


async def kick_member(ralsei_bot, database_manager: RalseiBotDatabaseManager, member: discord.Member,
                      reason: str = "The Kick Hammer Has Spoken!"):
    await general_action(member.kick, "kicked", ralsei_bot, database_manager, member, reason)


async def ban_member(ralsei_bot, database_manager: RalseiBotDatabaseManager, member: discord.Member,
                     reason: str = "The Ban Hammer Has Spoken!"):
    await general_action(member.ban, "banned", ralsei_bot, database_manager, member, reason)


def unban_member(ralsei_bot, database_manager: RalseiBotDatabaseManager, member: discord.Member):
    pass
