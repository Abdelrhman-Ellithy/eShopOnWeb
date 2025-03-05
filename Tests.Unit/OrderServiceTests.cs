public class OrderServiceTests{
        
    private readonly Mock<IRepository<Order>> orderRepository;
    private readonly Mock<IUriComposer> uriComposer;
    private readonly Mock<IRepository<Basket>> basketRepository;
    private readonly Mock<IRepository<CatalogItem>> itemRepository;
    private readonly OrderService orderService;

    public OrderServiceTests(){
        orderRepository = new Mock<IRepository<Order>>();
        uriComposer = new Mock<IUriComposer>();
        basketRepository = new Mock<IRepository<Basket>>();
        itemRepository = new Mock<IRepository<CatalogItem>>();
        orderService = new OrderService(basketRepository.Object, itemRepository.Object, orderRepository.Object, uriComposer.Object);
    }

}