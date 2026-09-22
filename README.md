[suivi de la premiere semaine](Doc/Suivi1.md) | [README de la fin de la V1](Doc/FinV1.md)

## Première version du projet — V1

Pour notre première version, on pensait faire quelque chose de simple afin d'avoir une première base fonctionnelle du projet avant de commencer la partie multi-robots et la coordination.

L'objectif de cette V1 est d'avoir :

- un entrepôt déjà présent dans Unity ;
- un seul robot ;
- une seule commande ;
- un seul produit à récupérer ;
- aucun problème de stock ;
- aucun conflit entre plusieurs robots.

Le but est simplement que le robot reçoive une commande, se déplace jusqu'à l'emplacement du produit, récupère le produit, puis l'amène jusqu'à la zone de dépôt.

Une fois le produit déposé, la commande est considérée comme terminée et le robot peut retourner à sa zone de départ.

### Scénario de la V1

Le déroulement prévu est le suivant :

```text
Création d'une commande
        ↓
La commande contient un produit
        ↓
Le produit possède un emplacement dans l'entrepôt
        ↓
Le robot reçoit la commande
        ↓
Le robot quitte la zone de départ
        ↓
Le robot se déplace dans l'entrepôt
        ↓
Le robot arrive à l'emplacement du produit
        ↓
Le robot récupère le produit
        ↓
Le robot se dirige vers la zone de dépôt
        ↓
Le robot dépose le produit
        ↓
La commande est terminée
        ↓
Le robot retourne à la zone de départ
```

---

## Structure du projet

Pour cette première version, nous avons choisi de garder une structure assez simple.

Elle pourra évoluer au fur et à mesure que nous ajouterons les différentes fonctionnalités du projet.

```text
Projet/
├── Assets/
│   ├── Scenes/
│   │   └── Entrepot.unity
│   │
│   ├── Prefabs/
│   │   ├── Robot.prefab
│   │   └── ZoneStock.prefab
│   │
│   ├── Materials/
│   │
│   ├── NavMesh/
│   │
│   ├── Scripts/
│   │   ├── Warehouse/
│   │   │   ├── Warehouse.cs
│   │   │   ├── Zone.cs
│   │   │   └── Emplacement.cs
│   │   │
│   │   ├── Orders/
│   │   │   ├── Commande.cs
│   │   │   └── Piece.cs
│   │   │
│   │   ├── Robots/
│   │   │   ├── RobotController.cs
│   │   │   └── RobotState.cs
│   │   │
│   │   └── Navigation/
│   │       └── RobotNavigation.cs
│   │
│   └── Editor/
│
├── Doc/
│   └── diagramme-classes-v1.png
│
└── README.md
```

Cette structure correspond pour l'instant uniquement aux besoins de notre V1.

## Diagramme de classes — V1

Avant de commencer l'implémentation, nous avons réalisé un premier diagramme de classes correspondant aux besoins de cette première version.

Les principales classes prévues sont :

- `Robot` : représente le robot chargé d'exécuter la commande ;
- `Commande` : représente une commande contenant un ou plusieurs produits ;
- `Piece` : représente un produit présent dans l'entrepôt ;
- `Emplacement` : représente la position d'un produit dans l'entrepôt ;
- `Zone` : représente les différentes zones de circulation dans l'entrepôt.

Le diagramme suivant représente la structure prévue pour notre V1.

### Diagramme UML

![Diagramme de classes V1](Doc/diagramme-classes-v1.png)

Ce diagramme représente uniquement notre première version. Il sera modifié et complété au fur et à mesure de l'ajout des nouvelles fonctionnalités.
