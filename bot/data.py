import json
import logging
import os
import random
from pathlib import Path
from typing import Dict

ralsei_quote_logger = logging.getLogger("RalseiQoutes")


# shut your bitch ass up
# noinspection SpellCheckingInspection
class RalseiDataManager:
    """
    This manages the quotes for the Ralsei bot's interactions and other messages.
    It imports all the quotes from JSON files and arranges them into a dictionary from where the
    other parts of the program can take from.
    """

    def load_all_quotes(self):
        for file in os.listdir("./all_data"):
            loaded_quotes = json.load(open(f"./all_data/{file}"))
            self.data[Path(file).stem] = loaded_quotes

    def __init__(self):
        self.data: Dict[str, list[str]] = {}
        self.load_all_quotes()

    def get_data_by_key(self, key: str) -> list[str]:
        """
        Get all the quotes associated with the given key.
        :param key: The key of the quotes.
        """
        return self.data[key]

    def get_data_by_key_rand(self, key: str) -> str:
        """
        Pick a random qoute associated with the given qoute key.
        :param key: The key of the qoutes.
        """
        return random.choice(self.get_data_by_key(key))
