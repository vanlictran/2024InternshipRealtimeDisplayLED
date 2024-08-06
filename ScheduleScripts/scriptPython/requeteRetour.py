import requests

# Fichier pour récupérer le temps de trajet entre 2 stations
# Le temps de trajet est majoré de 1,5 min pour simuler un possible arrêt ou un possible ralentissement du trafic

api_key = '84b99f7b-02b8-405f-a3e6-999b6bfd0315'

# Liste des points avec leurs coordonnées (latitude, longitude)
points = {
    "Công Viên Biển Đông Võ Nguyên Giáp": (16.073393, 108.245352),
    "Lô A5 Phạm văn đồng": (16.069992, 108.240501),
    "Đối diện Số 124": (16.070896, 108.235592),
    "Bùng binh Cầu Sông Hàn": (16.072410, 108.232120),
    "Số 12 Đ. Lê Duẩn": (16.071461, 108.221448),
    "Đối diện số 76": (16.073291, 108.220841),
    "Số 134 - THCS nguyễn huệ": (16.074144, 108.216893),
    "Số 24": (16.073388, 108.213178),
    "Số 162 - THCS hoàng diệu": (16.072368, 108.208211),
    "Số 312 - 314": (16.071128, 108.201997),
    "Số 396": (16.071074, 108.198425),
    "Số 574A - bhxh thanh khê": (16.071216, 108.192406),
    "Số 674": (16.071368, 108.187993),
    "Số 856 - BHXH THANH KHÊ": (16.068407, 108.183283),
    "Số 154": (16.071937, 108.178799),
    "Số 44 - trường đh tdtt": (16.074162, 108.176764),
    "Số 58-56 Lý Thái Tông": (16.074500, 108.175363),
    "Số 26 Kinh Dương Vương": (16.072445, 108.172967),
    "Số 200 Kinh Dương Vương": (16.074939, 108.169152),
    "Số 78 Phùng Hưng": (16.073173, 108.166542),
    "Số 900 tôn đức thắng": (16.071521, 108.150466),
    "Bưu điện hòa khánh": (16.079631, 108.147143),
    "Số 234": (16.083088, 108.145777),
    "Số 290": (16.086465, 108.144305),
    "Số 122": (16.090842, 108.148456),
    "Khu Du Lịch Xuân Triều": (16.094982, 108.146029),
    "Đối Diện Bảng Cấm Tắm Biển Nguyễn Tất Thành": (16.101324, 108.139797),
    "Chung Cư Hòa Hiệp Nam": (16.108574, 108.132814)
}

# URL de l'API GraphHopper
url = 'https://graphhopper.com/api/1/route'

# Fonction pour envoyer une requête à GraphHopper et obtenir l'itinéraire
def get_route(start, end, api_key):
    params = {
        'point': [f'{start[0]},{start[1]}', f'{end[0]},{end[1]}'],
        'vehicle': 'car',  # Vous pouvez changer le type de véhicule (foot, bike, etc.)
        'locale': 'fr',
        'instructions': 'true',
        'calc_points': 'true',
        'key': api_key
    }
    response = requests.get(url, params=params)
    if response.status_code == 200:
        return response.json()
    else:
        print(f"Erreur lors de la requête: {response.status_code} {response.text}")
        return None

# Liste des noms de points pour conserver l'ordre
point_names = list(points.keys())

# Boucle sur chaque paire de points successifs
with open("../fichiersTxt/itineraireRetour.txt", "w", encoding="utf-8") as f:
    # Boucle sur chaque paire de points successifs
    for i in range(len(point_names) - 1):
        start_name = point_names[i]
        end_name = point_names[i + 1]
        start_point = points[start_name]
        end_point = points[end_name]
        f.write(f"Calcul de l'itinéraire de {start_name} à {end_name}...\n")
        route = get_route(start_point, end_point, api_key)
        if route:
            for path in route['paths']:
                time = round(((path['time']/1000)/60) + 0.5)
                f.write(f"Temps: {time} min\n")
            f.write("\n")
 