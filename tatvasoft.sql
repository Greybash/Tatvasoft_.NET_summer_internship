CREATE TABLE customers (
    customer_id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    phone VARCHAR(15),
    address TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
CREATE TABLE orders (
    order_id SERIAL PRIMARY KEY,
    customer_id INT NOT NULL,
    order_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    total_amount DECIMAL(10, 2) NOT NULL,
    status VARCHAR(20) DEFAULT 'pending',

    FOREIGN KEY (customer_id) REFERENCES customers(customer_id)
);
INSERT INTO customers (name, email, phone, address) VALUES
('Rajesh Kumar', 'rajesh.kumar@example.com', '9876543210', '23 MG Road, Bengaluru, Karnataka'),
('Priya Sharma', 'priya.sharma@example.com', '9123456789', '88 Nehru Nagar, Delhi'),
('Amit Verma', 'amit.verma@example.com', '9988776655', 'Plot 12, Sector 15, Noida, Uttar Pradesh'),
('Sunita Reddy', 'sunita.reddy@example.com', '9090909090', 'Flat 21, Jubilee Hills, Hyderabad, Telangana'),
('Vikram Desai', 'vikram.desai@example.com', '9876512345', '14 Dadar East, Mumbai, Maharashtra');

INSERT INTO orders (customer_id, order_date, total_amount, status) VALUES
(1, '2025-05-20 10:30:00', 1499.50, 'shipped'),
(2, '2025-05-21 14:45:00', 2599.99, 'pending'),
(3, '2025-05-22 11:15:00', 349.00, 'delivered'),
(1, '2025-05-23 09:00:00', 999.00, 'cancelled'),
(4, '2025-05-24 16:10:00', 4999.00, 'shipped'),
(5, '2025-05-25 17:30:00', 799.99, 'pending');

select * from customers;


SELECT o.*
FROM orders o
JOIN customers c ON o.customer_id = c.customer_id
WHERE c.name = 'Rajesh Kumar';

SELECT DISTINCT c.name, c.email
FROM customers c
JOIN orders o ON c.customer_id = o.customer_id
WHERE o.status = 'pending';


-- Change status of order_id = 2 to 'shipped'
UPDATE orders
SET status = 'shipped'
WHERE order_id = 2;


SELECT
    o.order_id,
    c.name AS customer_name,
    c.phone,
    o.order_date,
    o.total_amount,
    o.status
FROM orders o
JOIN customers c ON o.customer_id = c.customer_id
ORDER BY o.order_date DESC;


SELECT SUM(total_amount) AS total_revenue FROM orders;


SELECT
    c.name AS customer_name,
    COUNT(o.order_id) AS orders_count
FROM customers c
LEFT JOIN orders o ON c.customer_id = o.customer_id
GROUP BY c.name;

SELECT
    DATE(order_date) AS order_day,
    SUM(total_amount) AS daily_revenue,
    COUNT(order_id) AS daily_orders
FROM orders
GROUP BY DATE(order_date)
ORDER BY order_day;

SELECT
    c.name AS customer_name,
    SUM(o.total_amount) AS total_spent
FROM customers c
JOIN orders o ON c.customer_id = o.customer_id
GROUP BY c.name
ORDER BY total_spent DESC
LIMIT 1;


