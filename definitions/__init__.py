from datetime import timedelta, timezone

from better_profanity.utils import os

active_timezone = timezone(offset=timedelta(hours=3), name="Europe/Istanbul")
"""
The active timezone of Ralsei. Which is Thefirey33's timezone.
"""

ONE_YEAR = 365
"""
One year in days. For some reason, the python timedelta library can go up to days maximum only.
Nice.
"""
BACK_TO_SLEEP_RANDOMIZER_START = 3
"""
Randomizer starting value for the back to sleep timer.
"""
BACK_TO_SLEEP_RANDOMIZER_END = 7
"""
Randomizer ending value for the back to sleep timer.
"""
MESSAGE_TYPE_DIVISION = 20
"""
The second divide for the typing effect to be real.
"""
PAT_PAT_TYPES = ["patpat", "petpet", "pat", "pet"]
"""
When petpeting the rals, these are the responses that the bot expects.
"""


def get_trusted_id():
    """
    Get the trusted user's ID.
    :return Trusted id.
    """
    return int(os.environ["TRUSTED_USER"])
