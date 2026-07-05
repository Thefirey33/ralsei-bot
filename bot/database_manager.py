import logging
import sqlite3

# The database logger, responsible for logging database related actions.
database_logger = logging.getLogger("RalseiBotDatabase")


class RalseiBotDatabaseModal:
    """
    The general database modal.
    """

    server_id: int
    general_channel: int
    ralsei_channel: int
    moderation_channel: int

    def __init__(self, server_id: int, general_channel: int, ralsei_channel: int, moderation_channel: int):
        self.server_id = server_id
        self.general_channel = general_channel
        self.ralsei_channel = ralsei_channel
        self.moderation_channel = moderation_channel

    @classmethod
    def tuple_to_modal(cls, modal_tuple: tuple[int, int, int, int]) -> RalseiBotDatabaseModal:
        return RalseiBotDatabaseModal(modal_tuple[0], modal_tuple[1], modal_tuple[2], modal_tuple[3])

    def __str__(self):
        """
        Returns the string representation of the modal.
        This is used for development purposes.
        :return: String representation of the modal.
        """
        return f"Server ID: {self.server_id} | General Channel: {self.general_channel} | Moderation Channel: {self.moderation_channel} | Ralsei Channel: {self.ralsei_channel}"


class RalseiBotDatabaseManager:
    total_count: int
    """
    Total count of tables in the database.
    """

    def initialize_new_database(self):
        """
        Creates a blank table for all the servers to be appended in.
        """
        self.cur.execute('CREATE TABLE bot_database(server_id, general_channel, ralsei_channel, moderation_channel);')
        self.cur.execute('CREATE TABLE allowed_entry(id);')
        self.db.commit()

    def check_if_server_exists(self, server_id: int):
        """
        Checks if the server with the given name exists in the database.
        :param server_id: The server to check.
        :return: If the server exists.
        """
        result = self.cur.execute('SELECT count(*) FROM bot_database WHERE server_id = ?', [server_id])
        return result.fetchone()[0] > 0

    def add_allowed(self, member_id: int):
        """
        Adds a listing where the specified member is allowed to enter the server.
        :param member_id: The member with the specified id.
        """
        self.cur.execute('INSERT INTO allowed_entry(id) VALUES (?)', [member_id])
        self.db.commit()

    def is_member_allowed(self, member_id: int) -> bool:
        """
        Checks if the specified member is allowed to join the server.
        :param member_id: The member with the specified ID.
        :return: If the member is allowed to join the server.
        """
        result = self.cur.execute('SELECT * FROM allowed_entry WHERE id = ?', [member_id])
        return result.fetchone() is not None

    def get_server_information(self, server_id: int) -> RalseiBotDatabaseModal | None:
        result = self.cur.execute('SELECT * FROM bot_database WHERE server_id = ?', [server_id])
        fetched_result = result.fetchone()

        if fetched_result is None:
            return None
        return RalseiBotDatabaseModal.tuple_to_modal(fetched_result)

    def add_server_to_database(self, modal: RalseiBotDatabaseModal):
        database_logger.info("Committing server %s's information to database...", modal.server_id)
        self.cur.execute('INSERT INTO bot_database VALUES (?, ?, ?, ?);', [
            modal.server_id,
            modal.general_channel,
            modal.ralsei_channel,
            modal.moderation_channel
        ])
        self.db.commit()

    def fetch_total_table_count_in_db(self):
        result = self.cur.execute('SELECT count(*) FROM sqlite_master WHERE type=\'table\';')
        self.total_count = result.fetchone()[0]

    def __init__(self):
        """
        Initializes the database
        """
        self.db = sqlite3.connect("bot_information.db")
        self.cur = self.db.cursor()

        self.fetch_total_table_count_in_db()
        database_logger.info("Checking tables, total count is: %s", self.total_count)
        if self.total_count == 0:
            database_logger.info("Detected that database is empty, creating tables...")
            self.initialize_new_database()

    def close_database(self):
        """
        Closes the database connection
        """
        self.cur.close()
        self.db.close()
