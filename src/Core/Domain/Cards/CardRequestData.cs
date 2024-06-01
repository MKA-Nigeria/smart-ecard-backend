using Domain.Enums;
using System;

namespace Domain.Cards;

public class CardRequestData
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? MiddleName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Address { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string PhotoUrl { get; set; }
    public byte[]? PhotoImage { get; set; }
    public Gender Gender { get; set; }
    public CardRequestData(string firstName, string lastName, string middleName, DateTime dateOfBirth, string address, string? email, string? phoneNumber, string photoUrl, byte[] photoImage, Gender gender)
    {
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        MiddleName = middleName;
        DateOfBirth = dateOfBirth;
        Address = address ?? throw new ArgumentNullException(nameof(address));
        Email = email;
        PhoneNumber = phoneNumber;
        PhotoUrl = photoUrl ?? throw new ArgumentNullException(nameof(photoUrl));
        PhotoImage = photoImage;
        Gender = gender;
    }

    public CardRequestData()
    {
    }
}
