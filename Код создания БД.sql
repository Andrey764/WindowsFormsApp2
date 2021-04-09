CREATE DATABASE MyOnlineBoardGameTournament
use MyOnlineBoardGameTournament

create table Users
(
	ID int identity not null primary key,
	UserLogin NVARCHAR(50) not null unique check(UserLogin!=''),
	UserPassword NVARCHAR(50) not null check(UserPassword!=''),
)

create table Games
(
	ID int identity not null primary key,
	Gamer1 nvarchar(50) NOT NULL check(Gamer1!=''),
	Gamer2 nvarchar(50) NOT NULL check(Gamer2!=''),
	GameName nvarchar(50) NOT NULL check(GameName!=''),
	Winner nvarchar(50) NOT NULL check(Winner!=''),
)