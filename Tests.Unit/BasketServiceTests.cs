using Xunit;
using Moq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.eShopWeb.ApplicationCore.Specifications;

namespace Tests.Unit;

public class BasketServiceTests
{
    private readonly Basket basket;
    private readonly Mock< IRepository<Basket> > basketRepo;
    private readonly Mock< IAppLogger<BasketService> > logger;
    private readonly BasketService basketService;

    public BasketServiceTests()
    {
        basket = new Basket("testbuyer");
        basketRepo = new Mock< IRepository<Basket> >();
        logger = new Mock< IAppLogger<BasketService> >();
        basketService = new BasketService(basketRepo.Object, logger.Object);
    }

    [Fact]
    public void testAddItem(){
        basket.AddItem(20,50);
        Assert.Equal(1, basket.Items.Count);
        Assert.Equal(50, basket.Items.First().UnitPrice);
        Assert.Equal(1, basket.Items.First().Quantity);
        basket.Items.First().SetQuantity(0);
        basket.RemoveEmptyItems();
        Assert.Empty(basket.Items);
    }

    [Fact]
    public async Task AddItemToBasket_WhenBasketExists_UpdatesBasket()
    {
        int catalogItemId = 123;
        decimal unitPrice = 10.50m;
        basketRepo.Setup(x => x.GetByIdAsync(basket.Id))
            .ReturnsAsync(basket);
        await basketService.AddItemToBasket(basket.Id, catalogItemId, unitPrice);
        Assert.Single(basket.Items);
        basketRepo.Verify(x => x.UpdateAsync(basket), Times.Once);
    }

    [Fact]
    public async Task DeleteBasket_ShouldCallDeleteOnRepo()
    {
        var basketId = 123;
        basketRepo.Setup(x => x.DeleteAsync(It.IsAny<Basket>()))
            .Returns(Task.CompletedTask);
        await basketService.DeleteBasketAsync(basketId);
        basketRepo.Verify(x => x.DeleteAsync(It.IsAny<Basket>()), Times.Once);
    }

    [Fact]
    public async Task SetQuantities_WhenBasketExists_UpdatesQuantities()
    {
        var itemId = 123;
        var quantity = 3;
        basket.AddItem(itemId, 10.00m);
        basketRepo.Setup(x => x.GetByIdAsync(basket.Id))
            .ReturnsAsync(basket);

        var quantities = new Dictionary<string, int>
        {
            { basket.Items.First().Id.ToString(), quantity }
        };

        await basketService.SetQuantities(basket.Id, quantities);

        Assert.Equal(quantity, basket.Items.First().Quantity);
        basketRepo.Verify(x => x.UpdateAsync(basket), Times.Once);
    }

    [Fact]
    public async Task TransferBasket_WhenSourceBasketExists_TransfersItems()
    {
        string anonymousId = "anon-123";
        string userId = "user-123";
        var sourceBasket = new Basket(anonymousId);
        sourceBasket.AddItem(123, 10.00m);
        
        basketRepo.Setup(x => x.GetBySpecAsync(It.IsAny<BasketWithItemsSpecification>()))
            .ReturnsAsync(sourceBasket);

        await basketService.TransferBasketAsync(anonymousId, userId);
        basketRepo.Verify(x => x.AddAsync(It.Is<Basket>(b => 
            b.BuyerId == userId && 
            b.Items.Count == sourceBasket.Items.Count)), Times.Once);

        basketRepo.Verify(x => x.DeleteAsync(sourceBasket), Times.Once);
    }
}