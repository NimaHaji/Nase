CREATE TABLE Users (
                       Id INT NOT NULL PRIMARY KEY,
                       Email NVARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE Orders (
                        Id INT NOT NULL PRIMARY KEY,
                        UserId INT NOT NULL,
                        FOREIGN KEY (UserId) REFERENCES Users(Id)
);
