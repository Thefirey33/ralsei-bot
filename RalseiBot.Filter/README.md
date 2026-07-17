# Ralsei Bot NSFW Content Detection Engine

This section of the Ralsei Bot detects for harmful content in any discord message.

The backend will communicate with this API and send requests in the Aspire network.

It is made in Python 3.14, to allow for additional libraries like HuggingFace's `transformers` library.

It uses content classifiers, that detect if a following message contains NSFW, harmful or hateful media.