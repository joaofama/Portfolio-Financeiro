# 📈 Portfolio Financeiro API

API em .NET 8 desenvolvida para análise de portfólios de investimentos, cálculo de performance, análise de risco e sugestões de rebalanceamento.

## 🚀 Como Executar o Projeto

### Pré-requisitos
* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) instalado.

### Passos para rodar
1. Clone ou extraia este repositório.
2. Abra o terminal na pasta raiz do projeto (onde está o arquivo `.sln`).
3. Restaure as dependências e compile o projeto:
   ```bash
   dotnet build
4. Execute a API:
   ```bash
   dotnet run --project Portfolio-Financeiro

5. Acesse o Swagger para testar os endpoints: http://localhost:5000/swagger (ou a porta indicada no console).

🧪 Testes Unitários
O projeto conta com uma bateria de testes automatizados utilizando xUnit e Fluent Assertions (opcional), cobrindo regras de negócio e casos de borda.
Para rodar os testes:

Bash
dotnet test

🛠️ Decisões Técnicas & FAQ
Para atender aos requisitos de qualidade e às dúvidas do FAQ, as seguintes decisões foram tomadas:

Validação de Portfólio (404): A API valida a existência do userId. Caso não seja encontrado no SeedData.json, é retornado um 404 Not Found com uma mensagem clara, em conformidade com as boas práticas REST.

Ausência de Histórico de Preços: Se um ativo não possuir dados históricos para o cálculo de volatilidade, o campo Volatility retornará null. Isso garante que a API não forneça dados falsos ou imprecisos.

TargetAllocation != 100%: O sistema aceita a entrada e realiza a Normalização dos pesos internamente para o cálculo de rebalanceamento, garantindo que a sugestão de compra/venda seja matematicamente coerente com o capital total.

Injeção de Dependência: Utilizamos o tempo de vida Scoped para os serviços de cálculo, garantindo eficiência de memória e consistência durante o ciclo de vida de cada requisição HTTP.

🏗️ Arquitetura e Boas Práticas

SOLID: Aplicação rigorosa do princípio de Inversão de Dependência (D) através do uso de Interfaces.

Clean Code: Métodos pequenos, nomes de variáveis semânticos e tratamento de erros preventivo.

Separação de Camadas: Organização clara entre Controllers (orquestração), Services (lógica de negócio) e Data (acesso a dados).