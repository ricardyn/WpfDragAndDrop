# WpfApp.DragAndDrop

Um aplicativo **WPF** simples, em **.NET 9**, criado com o objetivo de **exemplificar a implementação da funcionalidade de Drag & Drop** entre dois controles `ListView`, seguindo o padrão **MVVM** e usando **Attached Behaviors** para manter o code-behind limpo.

> A ideia central é demonstrar como expor o Drag & Drop de forma **declarativa no XAML** e **testável no ViewModel**, sem acoplar lógica de UI à camada de apresentação.

---

## 🧭 Visão Geral

A janela principal exibe duas listas lado a lado:

| Coluna | ListView | Papel |
|--------|----------|-------|
| Esquerda | `ListViewBasket` (`Basket`) | **Drop Target** — recebe os itens arrastados |
| Direita | `ListViewGroceries` (`Groceries`) | **Drag Source** — origem dos itens arrastáveis |

O usuário arrasta um `GroceryItem` da lista da direita e o solta sobre a lista da esquerda. Dependendo de onde o item é solto, ele é:

1. **Adicionado** à cesta, caso ainda não exista um item com o mesmo nome;
2. **Acumulado em quantidade** caso seja solto sobre um `BasketItem` de mesmo nome;
3. **Acumulado em quantidade** caso seja solto sobre um espaço vazio e já exista um `BasketItem` de mesmo nome;
4. **Rejeitado com aviso** (`MessageBox`) caso seja solto sobre um `BasketItem` com nome diferente.

---

## 🗂 Estrutura do Projeto
```

WpfApp.DragAndDrop/
├── App.xaml / App.xaml.cs
├── WpfApp.DragAndDrop.csproj
└── MVVM/
├── Behaviors/
│   ├── DragSourceBehavior.cs     ← Lógica de "arrastar"
│   └── DropTargetBehavior.cs     ← Lógica de "soltar"
├── Core/
│   ├── DropPayloadModel.cs       ← Record com (DroppedItem, TargetItem)
│   ├── ObservableObject.cs       ← Base INotifyPropertyChanged/Changing
│   └── RelayCommand.cs           ← ICommand genérico
├── Models/
│   ├── BasketItem.cs             ← Item observável (cesta)
│   └── GroceryItem.cs            ← Item da prateleira
├── ViewModels/
│   └── MainWindowViewModel.cs
└── Views/
├── MainWindow.xaml
└── MainWindow.xaml.cs
```
---

## 🧠 Estratégia de Implementação

A funcionalidade de Drag & Drop foi dividida em **dois Attached Behaviors** independentes e reutilizáveis, evitando completamente o uso de code-behind para a lógica de arraste/soltura. A comunicação com o ViewModel acontece por meio de um **`ICommand` (RelayCommand)** que recebe um **payload imutável** descrito por um `record`:

```c#
public record DropPayload(object DroppedItem, object? TargetItem);
```



Isso permite que:

- A lógica de **detecção visual** (qual elemento está sob o cursor, hit-testing, mínima distância para iniciar drag, etc.) viva no Behavior, junto da UI;
- A lógica de **negócio** (o que fazer quando algo é solto) viva inteiramente no ViewModel, recebendo apenas um par `(item arrastado, item alvo)` desacoplado de qualquer detalhe de WPF.

### Por que Attached Behaviors?

WPF não expõe um suporte nativo elegante a Drag & Drop *MVVM-friendly*. Os eventos `PreviewMouseLeftButtonDown`, `MouseMove`, `DragOver` e `Drop` só existem no nível do `UIElement`. Encapsulá-los em **Attached Properties** traz três vantagens:

1. **Reutilização** — qualquer `ListView` (ou, com pequenas adaptações, qualquer `ItemsControl`) ganha a funcionalidade adicionando uma propriedade no XAML;
2. **Declarativo** — basta marcar a origem com `DragSourceBehavior.IsEnabled="True"` e o destino com `DropTargetBehavior.DropCommand="{Binding ...}"`;
3. **Sem code-behind** — `MainWindow.xaml.cs` permanece com apenas `InitializeComponent()`.

---

## 🎯 `DragSourceBehavior` — A Origem do Arraste

Responsável por iniciar a operação de drag a partir de um `ListView`.

### Propriedade anexada

```csharp
DragSourceBehavior.IsEnabled  // bool
```


Quando `IsEnabled = true`, o behavior se inscreve em dois eventos do `ListView`:

| Evento | Função |
|--------|--------|
| `PreviewMouseLeftButtonDown` | Captura o **ponto inicial** do clique (`_dragStartPoint`). |
| `MouseMove` | Verifica se o mouse foi movido com o botão pressionado **além** dos limites mínimos do sistema (`SystemParameters.MinimumHorizontal/VerticalDragDistance`). Se sim, inicia o `DragDrop.DoDragDrop`. |

### Como o item arrastado é identificado

Em vez de confiar apenas no `SelectedItem`, o behavior caminha pela **árvore visual** a partir de `e.OriginalSource` até encontrar um `ListViewItem`. Em seguida, obtém o modelo correspondente via:

```csharp
object draggedData = listView.ItemContainerGenerator.ItemFromContainer(item);
```


Esse modelo (`GroceryItem`) é embrulhado em um `DataObject` e passado a `DragDrop.DoDragDrop` com `DragDropEffects.All`. Como o `DataObject` foi criado a partir do próprio objeto, o tipo `GroceryItem` é automaticamente registrado como formato disponível para o `Drop`.

> 💡 **Detalhe importante:** o uso do limite mínimo de arraste evita disparar `DoDragDrop` em cliques simples, preservando comportamentos normais (seleção, foco, etc.).

---

## 🎯 `DropTargetBehavior` — O Alvo do Drop

Esse é o componente mais rico da solução. Ele expõe **três propriedades anexadas**:

| Propriedade | Tipo | Função |
|-------------|------|--------|
| `DropCommand` | `ICommand` | Comando do ViewModel a ser executado quando um item válido é solto. |
| `DroppedItemType` | `Type` | Tipo esperado do item **arrastado** (ex.: `GroceryItem`). Outros tipos são ignorados. |
| `TargetItemType` | `Type` | Tipo esperado do item **sob o cursor** no destino (ex.: `BasketItem`). |

A configuração no XAML é totalmente declarativa:

```xml
<ListView ItemsSource="{Binding Basket}"
          bh:DropTargetBehavior.DropCommand="{Binding DropItemCommand}"
          bh:DropTargetBehavior.TargetItemType="{x:Type models:BasketItem}"
          bh:DropTargetBehavior.DroppedItemType="{x:Type models:GroceryItem}" />
```


### O que acontece quando `DropCommand` é definido

No callback de mudança da propriedade, o behavior:

1. Define `AllowDrop = true` no elemento (requisito do WPF para receber drops);
2. Se inscreve nos eventos `DragOver` e `Drop`.

### Evento `DragOver` — feedback visual

Enquanto o mouse passa por cima do alvo durante o arraste, o método `_onDragOver`:

- Faz **hit testing** com `listView.InputHitTest(...)` para descobrir qual `ListViewItem` está sob o cursor;
- Define `IsSelected = true` nele, fornecendo um **destaque visual em tempo real** para o usuário enxergar onde o item será solto.

### Evento `Drop` — execução

A sequência executada em `_onDrop` é cuidadosamente projetada para ser segura e desacoplada:

```csharp
1. Verifica se DroppedItemType está configurado e se está presente no DataObject:
   e.Data.GetDataPresent(droppedItemType)

2. Extrai o item arrastado:
   droppedItem = e.Data.GetData(droppedItemType)

3. Descobre o item-alvo sob o cursor (pode ser null) e
   valida que ele é uma instância de TargetItemType:
   targetItemType.IsInstanceOfType(dataContext)

4. Constrói o payload imutável:
   var payload = new DropPayload(droppedItem, targetItem);

5. Consulta CanExecute e, se positivo, dispara o comando:
   command.Execute(payload);
```


### Por que checagens de `Type` em vez de genéricos?

Como `DependencyProperty` não trabalha bem com generics, expor `Type` como propriedade anexada é o padrão idiomático em WPF para esse tipo de filtragem. Isso mantém o behavior **agnóstico ao domínio**, permitindo reutilizá-lo em qualquer cenário (ex.: arrastar `Customer` para um `Order`, etc.).

---

## 🔁 Fluxo Completo de uma Operação de Drag & Drop

```
┌─────────────────────┐                ┌─────────────────────┐
│  ListView Groceries │                │   ListView Basket   │
│  (DragSource)       │                │   (DropTarget)      │
└──────────┬──────────┘                └──────────┬──────────┘
           │                                      │
   1. MouseDown → grava ponto                     │
   2. MouseMove > threshold                       │
   3. ItemContainerGenerator.ItemFromContainer    │
   4. DragDrop.DoDragDrop(new DataObject(item))   │
           │                                      │
           └──────────► WPF começa a operação ◄───┘
                                                  │
                              5. DragOver: hit test + IsSelected = true
                              6. Drop:
                                 - valida DroppedItemType
                                 - identifica TargetItem (BasketItem)
                                 - monta DropPayload
                                 - executa DropCommand
                                                  │
                                                  ▼
                                ┌─────────────────────────────┐
                                │   MainWindowViewModel       │
                                │   ._dropItem(payload)       │
                                │                             │
                                │   • já existe? soma Qty     │
                                │   • não existe? Add(...)    │
                                │   • nome diferente? warn    │
                                └─────────────────────────────┘
```


---

## 🧩 ViewModel — Regra de Negócio do Drop

`MainWindowViewModel.DropItemCommand` é um `RelayCommand` que recebe o `DropPayload` e aplica a regra:

| Situação | Ação |
|----------|------|
| Item não existe na cesta | Adiciona um novo `BasketItem` |
| Item solto sobre área vazia, mas já existe na cesta | Soma quantidade ao existente |
| Item solto sobre `BasketItem` de mesmo nome | Soma quantidade ao alvo |
| Item solto sobre `BasketItem` de nome diferente | Exibe `MessageBox` de aviso |

Repare que **o ViewModel não conhece nada de WPF além de `MessageBox`** (que poderia, em um cenário real, ser abstraído por um serviço de diálogo). Toda a manipulação visual fica nos behaviors.

---

## ▶️ Como Executar

Requisitos:

- **.NET 9 SDK** (Windows)
- Visual Studio 2022 (17.12+) ou JetBrains Rider

```shell script
dotnet run --project WpfApp.DragAndDrop.csproj
```


---

## 🧪 Como Estender

A arquitetura permite extensões diretas:

- **Suportar outros controles:** ajustar os `if (sender is not ListView)` para `ItemsControl` e usar `ItemsControl.ContainerFromElement(...)`.
- **Adicionar um adorner de preview:** dentro de `DragSourceBehavior._onMouseMove`, criar um `Adorner` para acompanhar o cursor antes de chamar `DoDragDrop`.
- **Suportar múltiplos tipos arrastáveis:** trocar `DroppedItemType` (Type único) por uma `IList<Type>` ou um conversor.
- **Cancelar visualmente drops inválidos:** em `_onDragOver`, definir `e.Effects = DragDropEffects.None` quando o tipo não corresponder.

---

## 📌 Resumo dos Pontos-Chave

- **Padrão MVVM** preservado: nenhum handler de mouse no code-behind.
- **Dois Attached Behaviors**: `DragSourceBehavior` (origem) e `DropTargetBehavior` (destino).
- **Comunicação via `ICommand` + `DropPayload`** (record imutável).
- **Filtragem por `Type`** via propriedades anexadas, mantendo os behaviors genéricos.
- **Hit testing manual** com `VisualTreeHelper`/`InputHitTest` para identificar containers de itens.
- **Feedback visual** em tempo real durante o `DragOver` via seleção do item sob o cursor.