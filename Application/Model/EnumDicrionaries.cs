using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace FederalProjectManager.Model
{
    public enum EParameterValueType
    {
        [Description("Не задан")]
        Undefined = 0,
        [Description("Дробное число")]
        Decimal = 1,
        [Description("Целое число")]
        Int32 = 2,
        [Description("Строка")]
        String = 3,
        [Description("Дата")]
        DateTIme = 4,
        [Description("Справочное значение")]
        Enum = 5,
        [Description("Путь к файлу")]
        Path = 6
    }
    public enum ERole
    {
        [Description("Администратор")]
        Administrator = 1,
        [Description("Дизайнер")]
        Designer = 2,
        [Description("Бухгалтер")]
        Accountant = 3,
        [Description("Художник-оформитель")]
        ShopWorker = 4,
        [Description("Монтажник")]
        Adjuster = 5
    }
    public enum EStageState
    {
        [Description("Неизменяемая")]
        Immutable = 0,
        [Description("В работе")]
        Procssing = 1,
        [Description("Выполнена")]
        Completed = 2,
        [Description("Отменена")]
        Canceled = 3,
        [Description("Пропущена")]
        Skipped = 4,
        [Description("Конфигурирование")]
        Configurating = 5
    }
    public enum ERowState
    {
        [Description("Неизменяемая")]
        Immutable = 0,
        [Description("Активная")]
        Active = 1,
        [Description("Удаленная")]
        Deleted = 2,
        [Description("Архивная")]
        Archived = 3
    }
    public enum EProjectPriority
    {
        [Description("Очень низкий")]
        VeryLow = 1,
        [Description("Низкий")]
        Low = 2,
        [Description("Средний")]
        Middle = 3,
        [Description("Высокий")]
        High = 4,
        [Description("Самый высокий")]
        VeryHigh = 5
    }
    public enum EStageTemplate
    {
        [Description("Договор")]
        Contract = 1,
        [Description("Оплата")]
        Payment = 2,
        [Description("Эскиз")]
        Sketch = 3,
        [Description("Отгрузка")]
        Shipment = 4
    }
    public enum EStatisticsTile
    {
        HighPriority = 1,
        Today = 2,
        Current = 3,
        Overstay = 4,
        ProductionCompleted = 5,
        Prepare = 6,
        Archive = 7
    }
    public enum EProjectState
    {
        [Description("Выполнен")]
        Completed = 1,
        [Description("В работе")]
        InWork = 2,
        [Description("Не отдан в работу")]
        NotInWork = 3
    }
}
