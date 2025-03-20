КР1. Лейбович Алёна Максимовна, БПИ235.

В данном проекте реализован функционал учета финансов, который включает возможность добавления счетов, транзакций и бюджетов. 

## Примененные принципы SOLID

• S: Каждый класс отвечает за одну задачу.
  
• O: Классы открыты для расширения, но закрыты для модификации.

• L: Подтипы могут быть заменены базовыми типами.

• I: Интерфейсы разбиты на более мелкие и специфичные.

• D: Высокоуровневые модули не зависят от низкоуровневых.

## Примененные принципы GRASP

1. **High Cohesion**: Каждый класс имеет четко определенные обязанности. Например, класс Operation отвечает только за операции.

2. **Low Coupling**: Классы взаимодействуют друг с другом через интерфейсы.

## Примененные паттерны GoF

**Factory Method**: Используется для создания объектов Operation. Это позволяет создавать различные типы операций без изменения кода клиента.

**Facade**: Обеспечивает упрощенный интерфейс для работы с различными компонентами системы.

**Command**: Используется для работы с операциями.
   
## Инструкция по запуску приложения

1. Склонируйте репозиторий.
2. Откройте проект в вашей IDE.
3. Соберите проект и запустите консольное приложение.
