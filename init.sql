-- Grant permissions to the application user
-- This is mainly for the tests to work properly
GRANT ALL PRIVILEGES ON *.* TO 'basket_user'@'%';
FLUSH PRIVILEGES;