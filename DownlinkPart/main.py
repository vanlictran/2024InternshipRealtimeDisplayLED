import requests
import base64
import time

# Étape 1 : Obtenir le temps des prochains bus de la station
def get_bus_times(station_name):
    url = f"http://localhost:8000/api/Position/{station_name}"
    response = requests.get(url)
    if response.status_code == 200:
        # Retourner la réponse brute en texte (pas de JSON ici)
        print(f"Prochain bus {station_name} : {response.text}")
        return response.text.strip()  # .strip() pour enlever les espaces blancs inutiles

    else:
        raise Exception(f"Failed to get data from {url}. Status code: {response.status_code}")


# Nouvelle étape : Obtenir la température actuelle à Danang
def get_danang_temperature():
    api_key = "de5ee61bee79d29bc43274e8aeffa960"
    url = f"http://api.openweathermap.org/data/2.5/weather?q=Danang,vn&APPID={api_key}&units=metric"

    response = requests.get(url)
    if response.status_code == 200:
        data = response.json()
        temperature = data['main']['temp']
        print(f"Température actuelle à Danang: {temperature}°C")
        return temperature
    else:
        raise Exception(f"Failed to get temperature data. Status code: {response.status_code}")


# Étape 2 : Convertir les données en Base64
def convert_to_base64(data):
    data_string = str(data)  # Convertir en chaîne de caractères
    base64_bytes = base64.b64encode(data_string.encode("utf-8"))
    return base64_bytes.decode("utf-8")

# Étape 3 : Envoyer les données encodées via une requête POST au serveur LoRaWAN
def send_lorawan_data(encoded_data):
    url = "http://api.vngalaxy.vn:3004/api/downlink"
    headers = {
        'Authorization': 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJkZXZFVUkiOiJiN2RhMDNkNGMxNWJlOGM5IiwiYXBwSUQiOiI4MSIsImVtYWlsIjoibG9uZy52dTY2MjBAZ21haWwuY29tIiwicGFzc3dvcmQiOiJMb25nMTIzQCIsImlhdCI6MTcyNDEyNTE5Nn0.RLuwVqMv8mnu9J_qAkvWi_jJa4gamuRs79bo8vL-i3k'
    }
    payload = {
        "devEUI": "b7da03d4c15be8c9",
        "confirmed": True,
        "data": encoded_data,
        "fPort": 1
    }

    response = requests.post(url, json=payload, headers=headers)
    if response.status_code == 200:
        print("Data sent successfully!")
    elif response.status_code == 400:
        print(f"Response content: {response.text}")
    else:
        raise Exception(f"Failed to send data to {url}. Status code: {response.status_code}")

if __name__ == "__main__":
    try:
        # Remplacer 'stationA' par le nom de la station souhaitée
        station_name = "stationA"

        while True:
            # Obtenir les temps des prochains bus
            bus_times = get_bus_times(station_name)

            # Obtenir la température actuelle à Danang
            danang_temperature = get_danang_temperature()

            combined_data = f"{bus_times} | {danang_temperature}°C"

            # Conversion en Base64
            encoded_combined_data = convert_to_base64(combined_data)
            print(f"Encoded Data: {encoded_combined_data}")

            # Envoi des données encodées via POST
            send_lorawan_data(encoded_combined_data)

            time.sleep(5)

    except Exception as e:
        print(f"An error occurred: {e}")
