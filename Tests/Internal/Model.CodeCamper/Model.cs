
using Breeze.Sharp;
using System;


namespace Model.CodeCamper {

  public class Person : BaseEntity {
    public int Id {
      get { return GetValue<int>(); }
      set { SetValue(value); }
    }
    public string FirstName {
      get { return GetValue<string>(); }
      set { SetValue(value); }
    }
    public string LastName {
      get { return GetValue<string>(); }
      set { SetValue(value); }
    }
    public string Email {
      get { return GetValue<string>(); }
      set { SetValue(value); }
    }
    public string Blog {
      get { return GetValue<string>(); }
      set { SetValue(value); }
    }
    public string Twitter {
      get { return GetValue<string>(); }
      set { SetValue(value); }
    }
    public string Gender {
      get { return GetValue<string>(); }
      set { SetValue(value); }
    }
    public string ImageSource {
      get { return GetValue<string>(); }
      set { SetValue(value); }
    }
    public NavigationSet<Session> SpeakerSessions {
      get { return GetValue<NavigationSet<Session>>(); }
      set { SetValue(value); }
    }
  }

  public class Session : BaseEntity {
    public int Id {
      get { return GetValue<int>(); }
      set { SetValue(value); }
    }
    public string Title {
      get { return GetValue<string>(); }
      set { SetValue(value); }
    }
    public string Code {
      get { return GetValue<string>(); }
      set { SetValue(value); }
    }
    public string Description {
      get { return GetValue<string>(); }
      set { SetValue(value); }
    }
    public string Level {
      get { return GetValue<string>(); }
      set { SetValue(value); }
    }
    public string Tags {
      get { return GetValue<string>(); }
      set { SetValue(value); }
    }
    public int RoomId {
      get { return GetValue<int>(); }
      set { SetValue(value); }
    }
    public int SpeakerId {
      get { return GetValue<int>(); }
      set { SetValue(value); }
    }
    public int TimeSlotId {
      get { return GetValue<int>(); }
      set { SetValue(value); }
    }
    public int TrackId {
      get { return GetValue<int>(); }
      set { SetValue(value); }
    }
    public Room Room {
      get { return GetValue<Room>(); }
      set { SetValue(value); }
    }
    public Person Speaker {
      get { return GetValue<Person>(); }
      set { SetValue(value); }
    }
    public TimeSlot TimeSlot {
      get { return GetValue<TimeSlot>(); }
      set { SetValue(value); }
    }
    public Track Track {
      get { return GetValue<Track>(); }
      set { SetValue(value); }
    }
  }

  public class Room : BaseEntity {
    public int Id {
      get { return GetValue<int>(); }
      set { SetValue(value); }
    }
    public string Name {
      get { return GetValue<string>(); }
      set { SetValue(value); }
    }
  }

  public class TimeSlot : BaseEntity {
    public int Id {
      get { return GetValue<int>(); }
      set { SetValue(value); }
    }
    public DateTime Start {
      get { return GetValue<DateTime>(); }
      set { SetValue(value); }
    }
    public bool IsSessionSlot {
      get { return GetValue<bool>(); }
      set { SetValue(value); }
    }
    public int Duration {
      get { return GetValue<int>(); }
      set { SetValue(value); }
    }
  }

  public class Track : BaseEntity {
    public int Id {
      get { return GetValue<int>(); }
      set { SetValue(value); }
    }
    public string Name {
      get { return GetValue<string>(); }
      set { SetValue(value); }
    }
  }

  public static class Config {

    public static void Initialize(MetadataStore metadataStore) {

      // Configure keys and relationships
      var personBuilder = new EntityTypeBuilder<Person>(metadataStore);
      personBuilder.DataProperty(person => person.Id).IsPartOfKey();

      var sessionBuilder = new EntityTypeBuilder<Session>(metadataStore);
      sessionBuilder.DataProperty(session => session.Id).IsPartOfKey().IsAutoIncrementing();
      sessionBuilder.NavigationProperty(session => session.Room)
        .HasForeignKey(session => session.RoomId);
      sessionBuilder.NavigationProperty(session => session.TimeSlot)
        .HasForeignKey(session => session.TimeSlotId);
      sessionBuilder.NavigationProperty(session => session.Track)
        .HasForeignKey(session => session.TrackId);
      sessionBuilder.NavigationProperty(session => session.Speaker)
        .HasForeignKey(session => session.SpeakerId)
        .HasInverse(speaker => speaker.SpeakerSessions);

      var roomBuilder = new EntityTypeBuilder<Room>(metadataStore);
      roomBuilder.DataProperty(room => room.Id).IsPartOfKey().IsAutoIncrementing();

      var timeSlotBuilder = new EntityTypeBuilder<TimeSlot>(metadataStore);
      timeSlotBuilder.DataProperty(timeSlot => timeSlot.Id).IsPartOfKey().IsAutoIncrementing();

      var trackBuilder = new EntityTypeBuilder<Track>(metadataStore);
      timeSlotBuilder.DataProperty(track => track.Id).IsPartOfKey().IsAutoIncrementing();

      // Configure constraints
      personBuilder.DataProperty(person => person.FirstName).IsRequired().MaxLength(50);
      personBuilder.DataProperty(person => person.LastName).IsRequired().MaxLength(50);


      sessionBuilder.DataProperty(session => session.TrackId).IsRequired();
      sessionBuilder.DataProperty(session => session.RoomId).IsRequired();
      sessionBuilder.DataProperty(session => session.SpeakerId).IsRequired();
      sessionBuilder.DataProperty(session => session.RoomId).IsRequired();

      sessionBuilder.DataProperty(session => session.Title).IsRequired().MaxLength(50);
      sessionBuilder.DataProperty(session => session.Description).MaxLength(4000);



    }
  }
}


