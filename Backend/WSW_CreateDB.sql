SQL Create DB Script:



CREATE TABLE IF NOT EXISTS denylist (
	hashedtoken varchar(50) primary key,
	exp int not null
);

CREATE TABLE IF NOT EXISTS processes (
	processname varchar(30) primary key,
	email varchar(30) NOT NULL,
	displayedname varchar(50) NOT NULL,
	sec int	NOT NULL,
	FOREIGN KEY (email)
	REFERENCES users (email)
	ON UPDATE cascade
);

CREATE TABLE IF NOT EXISTS users (
	email varchar(30) primary key,
	password varchar(100) NOT NULL,
	firstname varchar(20) NOT NULL,
	surname varchar(30) NOT NULL,
	role varchar(15) NOT NULL
);