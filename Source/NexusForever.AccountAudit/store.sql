-- store export for NexusForever.AccountAudit --store, run against the world database with mysql -B
SELECT oi.id, oi.groupId, (oi.visible AND og.visible) AS visible,
    (SELECT GROUP_CONCAT(c.categoryId) FROM store_offer_group_category c WHERE c.id = og.id) AS categories,
    REPLACE(oi.name, '\t', ' ') AS name,
    (SELECT GROUP_CONCAT(CONCAT(d.itemId, 'x', d.amount)) FROM store_offer_item_data d WHERE d.id = oi.id) AS items,
    (SELECT GROUP_CONCAT(CONCAT(p.currencyId, ':', p.price)) FROM store_offer_item_price p WHERE p.id = oi.id) AS prices
FROM store_offer_item oi
JOIN store_offer_group og ON og.id = oi.groupId
ORDER BY oi.id;
