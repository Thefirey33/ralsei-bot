"""
Module for funsies.
This is where some more realistic stuff is added in, to keep the bot like an actual moderator.
"""

import asyncio
import random

import discord
from discord import Status

from definitions import BACK_TO_SLEEP_RANDOMIZER_END, BACK_TO_SLEEP_RANDOMIZER_START
from logsystem import ralsei_bot_logger


async def apply_sleep():
    await asyncio.sleep(
        random.randint(BACK_TO_SLEEP_RANDOMIZER_START, BACK_TO_SLEEP_RANDOMIZER_END)
    )


async def simulate_ralsei_wake(ralsei_bot):
    """
    Simulate Ralsei waking up due to a notification.
    :param ralsei_bot: Ralsei bot instance.
    """

    if ralsei_bot.awake:
        return

    ralsei_bot_logger.info("Waking up Ralsei...")
    await ralsei_bot.change_presence(
        status=Status.online,
        activity=discord.Game(name="*woke up because got notified*"),
    )
    await apply_sleep()


async def simulate_ralsei_sleep(ralsei_bot):
    """
    Simulate ralsei going back to sleep due to a notification.
    :param ralsei_bot: The ralsei bot instance.
    """

    if ralsei_bot.awake:
        return

    await apply_sleep()
    ralsei_bot_logger.info("Sleeping Ralsei...")
    # After finishing the Easter Egg, continue sleepin'.
    await ralsei_bot.change_presence(
        status=Status.idle, activity=discord.Game(name="*fluffy boi is asleep again*")
    )
