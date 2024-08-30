# Real-Time LED Display Project for Bus Stations

## Overview
This project displays real-time bus location and weather data on LED panels at bus stations using LoRaWAN and an Arduino microcontroller.

## Setup Instructions

### 1. Start the Application Containers
Before anything else, ensure that Nicolas's application containers are running. This is necessary for the API to be available for data requests.

### 2. Run the Python Script
At the root of the project, there's a `main.py` file. This script:
- Sends requests to Nicolas's API.
- Retrieves weather data and sends it via LoRaWAN.

To run the script, execute:

```bash
python main.py


### 3. Compile and Upload to Arduino
Navigate to:

```bash
\DownlinkPart\UCA21-main\Code\LORAWAN\ABP\Basic\UCA-ABP_Basic

