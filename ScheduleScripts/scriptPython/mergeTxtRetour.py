import re
#Ce fichier sert 0 rassembler les informations présentes dans les différents fichiers
# il permet de créer un seul fichier avec les informations utiles pour les ajouter a la BDD plus tard. 

def extraireMinute():
    with open('../fichiersTxt/itineraireRetour.txt','r', encoding='utf-8') as fichier :
        contenu = fichier.read()
        tableau = re.findall(r'Temps: (\d+) min', contenu)
        return tableau

def infosStation(indice) :
     with open('../fichiersTxt/arretRetour.txt','r',encoding='utf-8') as arrets :
        contenuArret = arrets.readlines()
        station = contenuArret[indice]
        separation = station.split(":",1)
        separation2 = separation[0].split(",",1)
        separation2.append(separation[1])
        separation2 = [element.strip() for element in separation2]
        return separation2[0]+","+separation2[1]+":"+separation2[2]+":"

heure = 0
min = 0
nbtrajet = 0
fichierAEcrire ={}
trajet = extraireMinute()
with open('../fichiersTxt/departRetour.txt', 'r') as fichier:
    for ligne in fichier:         
        nbtrajet += 1
        tableau = ligne.split(":",1)
        heure = tableau[0]
        min = tableau[1]
       # print("--------Trajet",nbtrajet," -----------------------")
        
       # print(infosStation(0),heure, "h", min)
        if nbtrajet<=1 :
            fichierAEcrire[0] = infosStation(0)+heure+ "h"+ min 
        else :
             fichierAEcrire[0] +=","+ heure+ "h"+ min
        for i in range(len(trajet)) :
            min = int(min) + int(trajet[i])
            if min >=60 :
                min = min -60
                heure = int(heure)+ 1
            if min <10 :
                min = "0"+ str(min)
           # print(infosStation(i+1),heure, "h", min)
            if nbtrajet<=1 :
                fichierAEcrire[i+1]=infosStation(i+1)+str(heure)+ "h"+ str(min)
            else :
                fichierAEcrire[i+1] +=","+ str(heure)+ "h"+ str(min)
fichierAEcrire[0] = fichierAEcrire[0].replace('\n','')
print(fichierAEcrire)

with open('../fichiersTxt/outputRetour.txt', 'w', encoding='utf-8') as fichier:
    for element in fichierAEcrire:
         fichier.write(str(fichierAEcrire[element]) + '\n')

