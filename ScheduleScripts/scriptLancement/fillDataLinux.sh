#!/bin/bash
# Exécuter le premier script Python
cd ../scriptPython
python3 suppression.py

# Vérifier si le premier script a été exécuté avec succès
if [ $? -ne 0 ]; then
    echo "Erreur lors de l'exécution de suppression.py"
    exit 1
fi

# Exécuter le deuxième script Python
python3 addBDDAller.py
python3 addBDDRetour.py
# Vérifier si le deuxième script a été exécuté avec succès
if [ $? -ne 0 ]; then
    echo "Erreur lors de l'exécution de addBDD.py"
    exit 1
fi

echo "Les deux scripts ont été exécutés avec succès"
