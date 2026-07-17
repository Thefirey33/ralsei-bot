from pydantic import BaseModel


class MessageTextRequest(BaseModel):
    """
    The request for the filtration engine's requests to TEXT messages.
    """
    text: str


class MessageModerationResponse(BaseModel):
    """
    The request for the filtration engine's response to TEXT messages.
    """
    is_nsfw: bool
    is_hateful: bool
