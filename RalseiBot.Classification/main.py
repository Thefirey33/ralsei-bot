import io
import logging

from PIL import Image
from fastapi import FastAPI, UploadFile, HTTPException, status
from transformers import pipeline

from modeltypes import MessageTextRequest, MessageModerationResponse

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

offensive_speech_classifier = pipeline("text-classification", model="KoalaAI/OffensiveSpeechDetector")
nsfw_text_classifier = pipeline("text-classification", model="TostAI/nsfw-text-detection-large")
nsfw_image_classifier = pipeline("image-classification", model="Falconsai/nsfw_image_detection")
MIN_SCORE_MODERATION = 0.9
app = FastAPI()


@app.post("/filter_text")
async def filter_text(message_content_request: MessageTextRequest):
    """
    This filter-check the specified text to see if it contains NSFW media.
    :param message_content_request: The message content request.
    :return: If it contains NSFW media or not.
    """
    result_nsfw = nsfw_text_classifier(message_content_request.text)[0]
    result_offensive = offensive_speech_classifier(message_content_request.text)[0]

    logger.info(f"NSFW result of message is: {result_nsfw}")
    logger.info(f"OFFENSIVE result of message is: {result_offensive}")

    nsfw_category: int = int(result_nsfw["label"].split("_")[1])
    # If it's detected that it contains NSFW media, with a score over %90, it's NSFW.
    # If it's detected that it contains offensive media, it's OFFENSIVE.
    return MessageModerationResponse(
        is_nsfw=nsfw_category != 0,
        is_hateful=result_offensive["label"] == "offensive" and result_offensive["score"] > 0.7,
    )


@app.post("/filter_image")
async def filter_image(file: UploadFile):
    if file.content_type is None:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Content-Type header not provided"
        )

    if not file.content_type.startswith("image/"):
        return HTTPException(
            status_code=status.HTTP_415_UNSUPPORTED_MEDIA_TYPE,
            detail="Must provide an image"
        )

    file_content = await file.read()
    image_data = Image.open(io.BytesIO(file_content))

    # Run the detection classifier
    results = nsfw_image_classifier(image_data)

    top_result = results[0]

    return MessageModerationResponse(
        is_nsfw=top_result["label"] == "nsfw" and top_result["score"] > MIN_SCORE_MODERATION
    )
