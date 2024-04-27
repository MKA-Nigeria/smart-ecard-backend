using Domain.Enums;
using System;

namespace Domain.Cards;

public class CardRequestData
{
    public string FirstName { get; }
    public string LastName { get; }
    public DateTime DateOfBirth { get; }
    public string Address { get; }
    public string? Email { get; }
    public string? PhoneNumber { get; }
    public string PhotoUrl { get; }
    public Gender Gender { get; }
    public CardRequestData(string firstName, string lastName, DateTime dateOfBirth, string address, string? email, string? phoneNumber, string photoUrl, Gender gender)
    {
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        DateOfBirth = dateOfBirth;
        Address = address ?? throw new ArgumentNullException(nameof(address));
        Email = email;
        PhoneNumber = phoneNumber;
        PhotoUrl = photoUrl ?? throw new ArgumentNullException(nameof(photoUrl));
        Gender = gender;
    }

    private CardRequestData()
    {
    }
}
