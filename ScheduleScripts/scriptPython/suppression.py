from influxdb_client import InfluxDBClient, Point, WritePrecision
from influxdb_client.client.write_api import SYNCHRONOUS
import os, time
import datetime
url = "http://localhost:8086"  # Remplacez par l'URL de votre instance InfluxDB
token = "Xip2vb7g32N9seLG5mOSynBzRSZuY2zRjyNhcVMyTi407UkamsXkepFZAUP-EXdzfQkCiVYns9m2zQyuU-zBkw=="
org = "stationBus"
bucket = "bucketStation"

write_client = InfluxDBClient(url=url, token=token, org=org)
write_api = write_client.write_api(write_options=SYNCHRONOUS)
delete_api = write_client.delete_api()
def supprimer_donnees():
    start = "1970-01-01T00:00:00Z"
    stop = "2024-12-31T23:59:59Z"
    predicate1 = '_measurement="bus_stations_infosAller"'
    predicate2 = '_measurement="bus_stations_infosRetour"'
    delete_api.delete(start, stop, predicate1, bucket=bucket, org=org)
    delete_api.delete(start, stop, predicate2, bucket=bucket, org=org)
    print("Les données ont été supprimées avec succès.")

# Appeler la fonction pour supprimer les données
supprimer_donnees()
