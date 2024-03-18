using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL;

// ReSharper disable once UnusedMember.Global
public class LazyLoadProxyNpgsqlTest : LazyLoadProxyTestBase<LazyLoadProxyNpgsqlTest.LoadNpgsqlFixture>
{
    public LazyLoadProxyNpgsqlTest(LoadNpgsqlFixture fixture)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
    }

    [ConditionalFact] // Requires MARS
    public override void Top_level_projection_track_entities_before_passing_to_client_method() { }

    [ConditionalTheory(Skip = "Possibly requires MARS, investigate")]
    public override void Lazy_load_one_to_one_reference_with_recursive_property(EntityState state)
        => base.Lazy_load_one_to_one_reference_with_recursive_property(state);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    protected override void RecordLog()
        => Sql = Fixture.TestSqlLoggerFactory.Sql;

    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    private string Sql { get; set; }

    #region Expected JSON override

    // TODO: Tiny discrepancy in decimal representation (Charge: 1.0 instead of 1.00)
    protected override string SerializedBlogs2
        => """
{
  "$id": "1",
  "$values": [
    {
      "$id": "2",
      "Id": 1,
      "Writer": {
        "$id": "3",
        "FirstName": "firstNameWriter0",
        "LastName": "lastNameWriter0",
        "Alive": false,
        "Culture": {
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "4",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "5",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.0,
            "Tag": {
              "$id": "6",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        },
        "Milk": {
          "$id": "7",
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "8",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "9",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.0,
            "Tag": {
              "$id": "10",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        }
      },
      "Reader": {
        "$id": "11",
        "FirstName": "firstNameReader0",
        "LastName": "lastNameReader0",
        "Alive": false,
        "Culture": {
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "12",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "13",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.0,
            "Tag": {
              "$id": "14",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        },
        "Milk": {
          "$id": "15",
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "16",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "17",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.0,
            "Tag": {
              "$id": "18",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        }
      },
      "Host": {
        "$id": "19",
        "HostName": "127.0.0.1",
        "Rating": 0,
        "Culture": {
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "20",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "21",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.0,
            "Tag": {
              "$id": "22",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        },
        "Milk": {
          "$id": "23",
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "24",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "25",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.0,
            "Tag": {
              "$id": "26",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        }
      },
      "Culture": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "$id": "27",
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "$id": "28",
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.0,
          "Tag": {
            "$id": "29",
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "Milk": {
        "$id": "30",
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "$id": "31",
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "$id": "32",
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.0,
          "Tag": {
            "$id": "33",
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      }
    },
    {
      "$id": "34",
      "Id": 2,
      "Writer": {
        "$id": "35",
        "FirstName": "firstNameWriter1",
        "LastName": "lastNameWriter1",
        "Alive": false,
        "Culture": {
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "36",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "37",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.0,
            "Tag": {
              "$id": "38",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        },
        "Milk": {
          "$id": "39",
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "40",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "41",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.0,
            "Tag": {
              "$id": "42",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        }
      },
      "Reader": {
        "$id": "43",
        "FirstName": "firstNameReader1",
        "LastName": "lastNameReader1",
        "Alive": false,
        "Culture": {
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "44",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "45",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.0,
            "Tag": {
              "$id": "46",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        },
        "Milk": {
          "$id": "47",
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "48",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "49",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.0,
            "Tag": {
              "$id": "50",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        }
      },
      "Host": {
        "$id": "51",
        "HostName": "127.0.0.2",
        "Rating": 0,
        "Culture": {
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "52",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "53",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.0,
            "Tag": {
              "$id": "54",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        },
        "Milk": {
          "$id": "55",
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "56",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "57",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.0,
            "Tag": {
              "$id": "58",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        }
      },
      "Culture": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "$id": "59",
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "$id": "60",
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.0,
          "Tag": {
            "$id": "61",
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "Milk": {
        "$id": "62",
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "$id": "63",
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "$id": "64",
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.0,
          "Tag": {
            "$id": "65",
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      }
    },
    {
      "$id": "66",
      "Id": 3,
      "Writer": {
        "$id": "67",
        "FirstName": "firstNameWriter2",
        "LastName": "lastNameWriter2",
        "Alive": false,
        "Culture": {
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "68",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "69",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.0,
            "Tag": {
              "$id": "70",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        },
        "Milk": {
          "$id": "71",
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "72",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "73",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.0,
            "Tag": {
              "$id": "74",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        }
      },
      "Reader": {
        "$id": "75",
        "FirstName": "firstNameReader2",
        "LastName": "lastNameReader2",
        "Alive": false,
        "Culture": {
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "76",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "77",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.0,
            "Tag": {
              "$id": "78",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        },
        "Milk": {
          "$id": "79",
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "80",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "81",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.0,
            "Tag": {
              "$id": "82",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        }
      },
      "Host": {
        "$id": "83",
        "HostName": "127.0.0.3",
        "Rating": 0,
        "Culture": {
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "84",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "85",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.0,
            "Tag": {
              "$id": "86",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        },
        "Milk": {
          "$id": "87",
          "Species": "S1",
          "Subspecies": null,
          "Rating": 8,
          "Validation": false,
          "Manufacturer": {
            "$id": "88",
            "Name": "M1",
            "Rating": 7,
            "Tag": {
              "$id": "89",
              "Text": "Ta2"
            },
            "Tog": {
              "Text": "To2"
            }
          },
          "License": {
            "Title": "Ti1",
            "Charge": 1.0,
            "Tag": {
              "$id": "90",
              "Text": "Ta1"
            },
            "Tog": {
              "Text": "To1"
            }
          }
        }
      },
      "Culture": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "$id": "91",
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "$id": "92",
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.0,
          "Tag": {
            "$id": "93",
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "Milk": {
        "$id": "94",
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "$id": "95",
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "$id": "96",
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.0,
          "Tag": {
            "$id": "97",
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      }
    }
  ]
}
""";

    protected override string SerializedBlogs1
        => """
[
  {
    "Writer": {
      "Culture": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.0,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "Milk": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.0,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "FirstName": "firstNameWriter0",
      "LastName": "lastNameWriter0",
      "Alive": false
    },
    "Reader": {
      "Culture": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.0,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "Milk": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.0,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "FirstName": "firstNameReader0",
      "LastName": "lastNameReader0",
      "Alive": false
    },
    "Host": {
      "Culture": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.0,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "Milk": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.0,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "HostName": "127.0.0.1",
      "Rating": 0.0
    },
    "Culture": {
      "Species": "S1",
      "Subspecies": null,
      "Rating": 8,
      "Validation": false,
      "Manufacturer": {
        "Name": "M1",
        "Rating": 7,
        "Tag": {
          "Text": "Ta2"
        },
        "Tog": {
          "Text": "To2"
        }
      },
      "License": {
        "Title": "Ti1",
        "Charge": 1.0,
        "Tag": {
          "Text": "Ta1"
        },
        "Tog": {
          "Text": "To1"
        }
      }
    },
    "Milk": {
      "Species": "S1",
      "Subspecies": null,
      "Rating": 8,
      "Validation": false,
      "Manufacturer": {
        "Name": "M1",
        "Rating": 7,
        "Tag": {
          "Text": "Ta2"
        },
        "Tog": {
          "Text": "To2"
        }
      },
      "License": {
        "Title": "Ti1",
        "Charge": 1.0,
        "Tag": {
          "Text": "Ta1"
        },
        "Tog": {
          "Text": "To1"
        }
      }
    },
    "Id": 1
  },
  {
    "Writer": {
      "Culture": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.0,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "Milk": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.0,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "FirstName": "firstNameWriter1",
      "LastName": "lastNameWriter1",
      "Alive": false
    },
    "Reader": {
      "Culture": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.0,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "Milk": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.0,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "FirstName": "firstNameReader1",
      "LastName": "lastNameReader1",
      "Alive": false
    },
    "Host": {
      "Culture": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.0,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "Milk": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.0,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "HostName": "127.0.0.2",
      "Rating": 0.0
    },
    "Culture": {
      "Species": "S1",
      "Subspecies": null,
      "Rating": 8,
      "Validation": false,
      "Manufacturer": {
        "Name": "M1",
        "Rating": 7,
        "Tag": {
          "Text": "Ta2"
        },
        "Tog": {
          "Text": "To2"
        }
      },
      "License": {
        "Title": "Ti1",
        "Charge": 1.0,
        "Tag": {
          "Text": "Ta1"
        },
        "Tog": {
          "Text": "To1"
        }
      }
    },
    "Milk": {
      "Species": "S1",
      "Subspecies": null,
      "Rating": 8,
      "Validation": false,
      "Manufacturer": {
        "Name": "M1",
        "Rating": 7,
        "Tag": {
          "Text": "Ta2"
        },
        "Tog": {
          "Text": "To2"
        }
      },
      "License": {
        "Title": "Ti1",
        "Charge": 1.0,
        "Tag": {
          "Text": "Ta1"
        },
        "Tog": {
          "Text": "To1"
        }
      }
    },
    "Id": 2
  },
  {
    "Writer": {
      "Culture": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.0,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "Milk": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.0,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "FirstName": "firstNameWriter2",
      "LastName": "lastNameWriter2",
      "Alive": false
    },
    "Reader": {
      "Culture": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.0,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "Milk": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.0,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "FirstName": "firstNameReader2",
      "LastName": "lastNameReader2",
      "Alive": false
    },
    "Host": {
      "Culture": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.0,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "Milk": {
        "Species": "S1",
        "Subspecies": null,
        "Rating": 8,
        "Validation": false,
        "Manufacturer": {
          "Name": "M1",
          "Rating": 7,
          "Tag": {
            "Text": "Ta2"
          },
          "Tog": {
            "Text": "To2"
          }
        },
        "License": {
          "Title": "Ti1",
          "Charge": 1.0,
          "Tag": {
            "Text": "Ta1"
          },
          "Tog": {
            "Text": "To1"
          }
        }
      },
      "HostName": "127.0.0.3",
      "Rating": 0.0
    },
    "Culture": {
      "Species": "S1",
      "Subspecies": null,
      "Rating": 8,
      "Validation": false,
      "Manufacturer": {
        "Name": "M1",
        "Rating": 7,
        "Tag": {
          "Text": "Ta2"
        },
        "Tog": {
          "Text": "To2"
        }
      },
      "License": {
        "Title": "Ti1",
        "Charge": 1.0,
        "Tag": {
          "Text": "Ta1"
        },
        "Tog": {
          "Text": "To1"
        }
      }
    },
    "Milk": {
      "Species": "S1",
      "Subspecies": null,
      "Rating": 8,
      "Validation": false,
      "Manufacturer": {
        "Name": "M1",
        "Rating": 7,
        "Tag": {
          "Text": "Ta2"
        },
        "Tog": {
          "Text": "To2"
        }
      },
      "License": {
        "Title": "Ti1",
        "Charge": 1.0,
        "Tag": {
          "Text": "Ta1"
        },
        "Tog": {
          "Text": "To1"
        }
      }
    },
    "Id": 3
  }
]
""";

    #endregion Expected JSON override

    public class LoadNpgsqlFixture : LoadFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            // We default to mapping DateTime to 'timestamp with time zone', but the seeding data has Unspecified DateTimes which aren't
            // supported.
            modelBuilder.Entity<Quest>().Property(q => q.Birthday).HasColumnType("timestamp without time zone");
            modelBuilder.Entity<Parson>().Property(q => q.Birthday).HasColumnType("timestamp without time zone");
        }
    }
}
