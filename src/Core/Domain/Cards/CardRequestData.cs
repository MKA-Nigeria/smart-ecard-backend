using Domain.Enums;
using System;

namespace Domain.Cards;

public class CardRequestData
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Address { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string PhotoUrl { get; set; }
    public Gender Gender { get; set; }
    public string? BloodGroup{ get; set; }
    public string? Genotype{ get; set; }
    public CardRequestData(string firstName, string lastName, DateTime dateOfBirth, string address, string? email, string? phoneNumber, string photoUrl, Gender gender, string? bloodGroup, string? genotype)
    {
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        DateOfBirth = dateOfBirth;
        Address = address ?? throw new ArgumentNullException(nameof(address));
        Email = email;
        PhoneNumber = phoneNumber;
        PhotoUrl = photoUrl ?? throw new ArgumentNullException(nameof(photoUrl));
        Gender = gender;
        BloodGroup = bloodGroup;
        Genotype = genotype;
    }

    public CardRequestData()
    {
    }
}
