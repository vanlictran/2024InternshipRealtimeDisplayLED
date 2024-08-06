@echo off
REM Exécuter le premier script Python
cd ..\scriptPython
python suppression.py

REM Vérifier si le premier script a été exécuté avec succès
IF %ERRORLEVEL% NEQ 0 (
    echo "Erreur lors de l'exécution de suppression.py"
    EXIT /B %ERRORLEVEL%
)

REM Exécuter le deuxième script Python
python addBDDAller.py
python addBDDRetour.py
REM Vérifier si le deuxième script a été exécuté avec succès
IF %ERRORLEVEL% NEQ 0 (
    echo "Erreur lors de l'exécution de addBDD.py"
    EXIT /B %ERRORLEVEL%
)

echo "Les deux scripts ont été exécutés avec succès"
