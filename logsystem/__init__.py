import logging

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(name)s: %(message)s",
    datefmt="%Y-%m-%d %H:%M:%S",
)
ralsei_bot_logger = logging.getLogger("RalseiBot")


def initialize():
    """
    Logging initialization.
    :return: None
    """
    ralsei_bot_logger.info("Initialized Logging Subsystem..")
