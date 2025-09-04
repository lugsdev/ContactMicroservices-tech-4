-- Script de criação do banco de dados ContactsDB
-- Execute este script no SQL Server Management Studio ou Azure Data Studio

-- Criar banco de dados se não existir
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ContactsDB')
BEGIN
    CREATE DATABASE ContactsDB;
END
GO

-- Usar o banco de dados
USE ContactsDB;
GO

-- Criar tabela Contacts se não existir
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Contacts' AND xtype='U')
BEGIN
    CREATE TABLE Contacts (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nome NVARCHAR(100) NOT NULL,
        DDD NCHAR(2) NOT NULL,
        NumeroCelular NVARCHAR(9) NOT NULL,
        Email NVARCHAR(150) NOT NULL,
        DataCriacao DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        DataAtualizacao DATETIME2 NULL,
        
        -- Índices para melhor performance
        INDEX IX_Contacts_Email (Email),
        INDEX IX_Contacts_DDD_Numero (DDD, NumeroCelular),
        INDEX IX_Contacts_DataCriacao (DataCriacao)
    );
END
GO

-- Inserir dados de exemplo
IF NOT EXISTS (SELECT * FROM Contacts)
BEGIN
    INSERT INTO Contacts (Nome, DDD, NumeroCelular, Email, DataCriacao) VALUES
    ('João Silva', '11', '987654321', 'joao.silva@email.com', GETUTCDATE()),
    ('Maria Santos', '21', '876543210', 'maria.santos@email.com', GETUTCDATE()),
    ('Pedro Oliveira', '31', '765432109', 'pedro.oliveira@email.com', GETUTCDATE()),
    ('Ana Costa', '41', '654321098', 'ana.costa@email.com', GETUTCDATE()),
    ('Carlos Ferreira', '51', '543210987', 'carlos.ferreira@email.com', GETUTCDATE());
END
GO

PRINT 'Banco de dados ContactsDB criado com sucesso!';
PRINT 'Tabela Contacts criada com dados de exemplo.';
GO

