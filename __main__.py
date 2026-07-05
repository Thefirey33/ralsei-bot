"""
Reorganized Ralsei Bot Code.
This is a reorganization of the code, from a single file for extended capability.
"""

import os

from dotenv import load_dotenv

import logsystem
from bot import RalseiBot

load_dotenv(verbose=True)

if __name__ == "__main__":
    logsystem.initialize()

    bot_session = RalseiBot()
    bot_session.run(os.environ["DISCORD_TOKEN"], log_handler=None)
    bot_session.database_manager.close_database()
