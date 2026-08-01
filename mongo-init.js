db = db.getSiblingDB("delivery");

db.createUser({
    user: "delivery_app",
    pwd: "DeliveryMongo123",
    roles: [
        {
            role: "readWrite",
            db: "delivery"
        }
    ],
    mechanisms: [
        "SCRAM-SHA-256"
    ]
});

db.createCollection("pedidos");

db.pedidos.createIndex({
    creado: -1
});

db.pedidos.createIndex({
    estado: 1,
    estadoDelivery: 1,
    creado: 1
});

print(
    "Usuario delivery_app y colección pedidos creados correctamente."
);