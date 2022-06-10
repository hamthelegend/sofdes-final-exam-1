using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SofdesQuiz3_1;
public class User
{
    public User(string fullName, string email, DateOnly birthdate, string gender, string address, int? id = null)
    {
        Id = id;
        FullName = fullName;
        Email = email;
        Birthdate = birthdate;
        Gender = gender;
        Address = address;
    }

    public int? Id { get; }
    public string FullName { get; }
    public string Email { get; }
    public DateOnly Birthdate { get; }
    public string Gender { get; }
    public string Address { get; }

    public UserEntity ToUserEntity()
    {
        var userEntity = new UserEntity()
        {
            FullName = FullName,
            Email = Email,
            Birthdate = Birthdate,
            Gender = Gender,
            Address = Address,
        };
        if (Id != null) userEntity.Id = (int)Id;
        return userEntity;
    }

}

[Table("UsersTable")]
public class UserEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public DateOnly Birthdate { get; set; }
    public string Gender { get; set; }
    public string Address { get; set; }
    
    public User ToUser()
    {
        return new User(FullName, Email, Birthdate, Gender, Address, Id);
    }
}

public class UsersContext : DbContext
{
    public DbSet<UserEntity> UserEntities { get; set; }

    public string DbPath { get; }

    public UsersContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "users.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={DbPath}");

}

public static class UsersDb
{
    public static User Get(int id)
    {
        var context = new UsersContext();
        var userEntity = context.UserEntities.Where(user => user.Id == id).FirstOrDefault();
        return userEntity?.ToUser();
    }

    public static List<User> GetAll(string search = "")
    {
        search = search.ToLower();
        var context = new UsersContext();
        var userEntities = context.UserEntities.ToList();
        return userEntities
            .Where(
                user =>
                user.Id.ToString().ToLower().Contains(search) ||
                user.FullName.ToLower().Contains(search) ||
                user.Email.ToLower().Contains(search) ||
                user.Gender.ToLower().Contains(search) ||
                user.Address.ToLower().Contains(search)
            )
            .Select(userEntity => userEntity.ToUser())
            .ToList();
    }

    public static void InsertUpdate(User user)
    {
        var context = new UsersContext();
        var userEntityOnDb = context.UserEntities.FirstOrDefault(userEntity => userEntity.Id == user.Id);
        if (userEntityOnDb == null)
        {
            var userEntity = user.ToUserEntity();
            context.Add(userEntity);
            context.SaveChanges();
        }
        else
        {
            userEntityOnDb.FullName = user.FullName;
            userEntityOnDb.Email = user.Email;
            userEntityOnDb.Birthdate = user.Birthdate;
            userEntityOnDb.Gender = user.Gender;
            userEntityOnDb.Address = user.Address;
            context.SaveChanges();
        }
    }

    public static bool Delete(int id)
    {
        var context = new UsersContext();
        var userEntityOnDb = context.UserEntities.FirstOrDefault(userEntity => userEntity.Id == id);
        if (userEntityOnDb == null)
        {
            return false;
        }
        context.Remove(userEntityOnDb);
        context.SaveChanges();
        return true;
    }
}