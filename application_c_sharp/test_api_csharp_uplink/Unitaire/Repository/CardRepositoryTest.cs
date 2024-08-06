using api_csharp_uplink.DB;
using api_csharp_uplink.Entities;
using api_csharp_uplink.Repository;
using Moq;

namespace test_api_csharp_uplink.Unitaire.Repository;

public class CardRepositoryTest
{
    private const string MeasurementCard = "card";
    private readonly Card _cardExpected01 = new ("0", 1);
    private readonly CardDb _cardDbExpected = new(){ DevEuiCard = "0", LineBus = 1 };
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestAdd()
    {
        Mock<IGlobalInfluxDb> mock = new();
        mock.Setup(globalInfluxDb => globalInfluxDb.Save(It.IsAny<CardDb>()))
            .ReturnsAsync(_cardDbExpected);
        CardRepository cardRepository = new(mock.Object);
        
        Card result = await cardRepository.Add(_cardExpected01);
        Assert.Equal(_cardExpected01, result);
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestModify()
    {
        Mock<IGlobalInfluxDb> mock = new();
        mock.Setup(globalInfluxDb => globalInfluxDb.Get<CardDb>(MeasurementCard, 
                $"   |> filter(fn: (r) => r.devEuiCard == \"{_cardExpected01.DevEuiCard}\")"))
            .ReturnsAsync([]);
        mock.Setup(globalInfluxDb => globalInfluxDb.Save(It.IsAny<CardDb>()))
            .ReturnsAsync(_cardDbExpected);
        
        Card result = await new CardRepository(mock.Object).Modify(_cardExpected01);
        Assert.Equal(_cardExpected01, result);
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestGetByDevEui()
    {
        Mock<IGlobalInfluxDb> mock = new();
        mock.SetupSequence(globalInfluxDb => globalInfluxDb.Get<CardDb>(MeasurementCard,
                $"   |> filter(fn: (r) => r.devEuiCard == \"{_cardExpected01.DevEuiCard}\")"))
            .ReturnsAsync([])
            .ReturnsAsync([new CardDb{DevEuiCard = _cardExpected01.DevEuiCard, LineBus = _cardExpected01.LineBus}]);
        
        CardRepository cardRepository = new(mock.Object);
        Card? result = await cardRepository.GetByDevEui(_cardExpected01.DevEuiCard);
        Assert.Null(result);
        
        result = await cardRepository.GetByDevEui(_cardExpected01.DevEuiCard);
        Assert.Equal(_cardExpected01, result);
    }
    
    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestGetAll()
    {
        Card cardExpected12 = new("1", 2);
        Mock<IGlobalInfluxDb> mock = new();
        mock.Setup(globalInfluxDb => globalInfluxDb.GetAll<CardDb>(MeasurementCard))
            .ReturnsAsync([new CardDb{DevEuiCard = _cardExpected01.DevEuiCard, LineBus = _cardExpected01.LineBus}, 
                new CardDb{DevEuiCard = cardExpected12.DevEuiCard, LineBus = cardExpected12.LineBus}]);

        List<Card> result = await new CardRepository(mock.Object).GetAll();
        Assert.Equal(2, result.Count);
        Assert.Equal(_cardExpected01, result[0]);
        Assert.Equal(cardExpected12, result[1]);
    }
}