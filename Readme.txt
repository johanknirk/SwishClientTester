This is a minimal testcase to reproduce error when trying to create a Swish payment with dotnet core Linux

You must have docker (linux containers mode) installed

Use buildDocker.cmd to publish and build a local docker image.

docker run -it <docker image id> /bin/bash

dotnet SwishClientTester.dll