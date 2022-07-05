using DocAggregator.API.Core.Models;
using System;
using System.IO;
using System.Linq;

namespace DocAggregator.API.Core
{
    public class InventoryRequest { }

    public class InventoryResponse : InteractorResponseBase
    {
        public MemoryStream ResultStream { get; set; }
    }

    public class InventoryInteractor : InteractorBase<InventoryResponse, InventoryRequest>
    {
        FormInteractor _former;
        IInventoryRepository _repo;

        public InventoryInteractor(FormInteractor former, IInventoryRepository repository, ILoggerFactory loggerFactory)
            : base(loggerFactory.GetLoggerFor<InventoryInteractor>())
        {
            _former = former;
            _repo = repository;
        }

        protected override void Handle(InventoryResponse response, InventoryRequest request)
        {
            Inventory inventory = _repo.GetInventory(0);
            if (inventory == null)
            {
                throw new ArgumentException("Запись инвентаризации не найдена.", nameof(request));
            }
            FormRequest formRequest = new FormRequest();
            formRequest.Inventory = inventory;
            FormResponse formResponse = _former.Handle(formRequest);
            if (formResponse.Success)
            {
                response.ResultStream = formResponse.ResultStream;
            }
            else
            {
                response.AddErrors(formResponse.Errors.ToArray());
            }
        }
    }
}
