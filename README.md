# AgroSim

Protótipo acadêmico de simulação e gestão de operações agrícolas em **Unity 2D e C#**, com interface em português.

> **Revisão: 28/10/2026.** A versão pública `main`, conferida no commit `4425a6e`, contém o MVP de cinco operações. A safra gerencial está implementada na cópia local de desenvolvimento e ainda não foi publicada. Código implementado não significa validação visual ou executável disponível.

## Modos disponíveis

| Modo | Cena | Estado |
| --- | --- | --- |
| MVP de operações | `Assets/AgroSim/Scenes/Gameplay.unity` | Base presente no GitHub; cinco operações e save local. |
| Safra gerencial | `Assets/AgroSim/Scenes/Prototypes/MetricMapIntegration.unity` | Implementação local em dez talhões e 250 ha; testes de lógica executados. |

### MVP de operações

Menu → fazenda → galpão → seleção do TR-01 e implemento → deslocamento → trabalho no talhão → decisão → resultado → retorno e confirmação na base.

O código contempla preparo do solo, plantio, fertilização, pulverização e colheita; combustível, desgaste, produtividade, economia e persistência local. Há acesso à oficina e ao abastecimento. Equipamentos e operações têm representações simplificadas.

### Safra gerencial local

As sete etapas são: preparo do solo; plantio e irrigação; adubação inicial; adubação intermediária; adubação final; colheita; comercialização.

- Plano reordenável, orçamento por etapa e avanço bloqueado até a conclusão dos serviços exigidos em todos os talhões.
- Pausa e velocidades 1x, 2x, 4x e 8x; decisões com efeitos no tempo ou na produtividade.
- Contratos opcionais com preços fictícios, pagamento na entrega, cobertura de déficit e venda ou armazenamento de excedente.
- Save da fila, execução e mercado; retomada pausada, sem progresso offline, e tratamento de falhas de gravação.
- Painéis de plano, contratos e resultado; seleção de talhão, visão geral e localização do trabalho.

O modo automatiza a execução gerencial. Irrigação, animações e percursos são simplificados; seus custos não alteram os recursos da campanha física anterior. O modo métrico legado e seus testes continuam separados.

## Executar no Editor

1. Use **Unity 6000.6.4f1**, conforme `ProjectSettings/ProjectVersion.txt`.
2. Adicione a pasta no Unity Hub e aguarde a importação dos assets e pacotes.
3. Em um clone da versão pública atual, abra `Assets/AgroSim/Scenes/Gameplay.unity`.
4. Na cópia local que contém a safra, abra `Assets/AgroSim/Scenes/Prototypes/MetricMapIntegration.unity`.
5. Para testar a safra sem usar o progresso normal, execute **AgroSim → Safra em sete etapas → Jogar com save isolado**.

A safra salva em `Application.persistentDataPath/AgroSim/StageCampaign/season-seven-stages.json`. Os saves dos modos anteriores são independentes.

## Verificação e Windows

Em 28/10/2026, passaram novamente os testes externos de progressão das sete etapas, **46 verificações de mercado** e **70 de integração/persistência**. A compilação externa C# terminou sem erros. O teste nativo também passou no Unity: frames reais, fila, pausa, orçamento único, 8x, sete etapas, decisões, contratos e fechamento. Capturas de plano (720p/1080p), contratos, execução, decisão e resultado foram inspecionadas. Isso não substitui o teste do executável distribuído.

O comando **AgroSim → Safra em sete etapas → Validar ciclo gerencial e capturar HUD** verifica o modo novo no Editor com save isolado. O roteiro está em `Docs/Testes-safra-sete-etapas.md` na cópia local.

**Cena de exportação:** `EditorBuildSettings.asset` e o exportador histórico `WindowsPrototypeBuild` selecionam `Gameplay.unity`. Esse exportador gera um Development Build `0.001v`, sem a safra gerencial. Para empacotar a safra, selecione explicitamente `MetricMapIntegration.unity` como cena inicial, Windows x86_64 e desative Development Build. Distribua a pasta completa gerada, não apenas o executável.

Em 28/10/2026 foi gerado localmente um build Windows x86_64 da safra, sem Development Build: resultado Succeeded, zero erros e 491 avisos de shaders. Ele deriva de alterações locais ainda não publicadas. Não há release publicada no GitHub na consulta desta data.

## Limitações e trabalho futuro

- MQTT e integração operacional com AgroTrack não estão implementados; registros locais não equivalem a integração remota.
- Não há safra seguinte automática nem avanço offline na safra gerencial.
- Preços, rendimentos, tempos e custos são fictícios, sem validação agronômica.
- Testes físicos do modo legado não validam automaticamente a apresentação da nova safra.

## Próximas atualizações — planejamento resumido

Trabalho futuro, sujeito a revisão de escopo e validação a cada entrega:

- **Estabilidade da versão Windows:** ampliar os testes de campanha completa no executável, recuperação de saves e compatibilidade; revisar os avisos de shaders.
- **Interface e orientação:** aprimorar legibilidade, navegação do mapa e clareza dos planos, decisões e resultados em diferentes resoluções.
- **Representação das operações:** refinar animações, irrigação, equipamentos e deslocamentos no mapa, mantendo coerência com a execução gerencial.
- **Equipamentos e manutenção:** evoluir a condição por peças e avaliar a inclusão de um segundo trator e de operações simultâneas.
- **Eventos operacionais:** ampliar decisões e consequências, com planejamento de falhas e recuperação de equipamentos.
- **Integração com AgroTrack:** definir e validar a troca de dados e a comunicação MQTT antes de incorporá-las ao jogo.
- **Consolidação e distribuição:** atualizar documentação, organizar versões e publicar novos pacotes após os testes e a aprovação de cada entrega.

## Organização

- `Assets/AgroSim/Scripts`: lógica, interface, operações, mercado e persistência.
- `Assets/AgroSim/Editor`: validadores, ferramentas e exportador histórico.
- `Assets/AgroSim/Scenes`: Gameplay e protótipos disponíveis conforme a versão.
- `Packages` e `ProjectSettings`: dependências e configuração Unity.
- `Docs`: implementação e testes; documentos históricos descrevem o estado de sua data.

Versione assets com seus arquivos `.meta`. O Unity recria `Library`; caches, builds, saves, logs e preferências locais não devem integrar o código-fonte publicado.

## Créditos e uso de IA

O pacote Cozy Farm foi utilizado como placeholder visual nas etapas iniciais e posteriormente removido do projeto. O registro de seu uso permanece em [Assets de terceiros](Docs/Assets-de-terceiros.md).

IA apoiou planejamento, código C#, testes, documentação e referências visuais. As decisões de escopo e a revisão dos resultados cabem ao desenvolvedor.

O repositório não possui licença declarada na consulta de 28/10/2026. A publicação do código não atribui automaticamente uma licença aos materiais próprios ou de terceiros.

### Verificação do pacote RC2

O executável Windows da prévia de 28/10/2026 abriu a safra com os dez talhões e a interface de planejamento. Foram corrigidas 19 malhas com marcação que as excluía do build. A verificação passou de 19 dependências excluídas para nenhuma; backups foram preservados. O pacote RC2 substitui a primeira tentativa, que falhava na inicialização. O ciclo completo foi validado no Editor; no executável foi realizado teste de abertura.
