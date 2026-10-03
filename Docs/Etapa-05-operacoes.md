# Etapa 05 — Operações e transformação do talhão

Escopo aprovado na conversa: preparo do solo, plantio, fertilização,
pulverização e colheita, usando placeholders. Serviços do galpão gratuitos.

O preparo transforma solo intacto em solo revirado com sulcos. O mecanismo
comum percorre faixas alternadas e transforma somente a área já trabalhada.
Plantio deixa sementes; fertilização deixa plantas e marca a aplicação;
pulverização mostra plantas desenvolvidas e névoa atrás do implemento;
colheita retira a cultura madura e deixa palha/solo colhido.

Uma operação libera a próxima apenas quando concluída e confirmada no galpão.
Interromper antes da confirmação restaura o último estado confirmado; não
salvar parcialmente o campo. A produtividade acumula as decisões da safra.
No fim da colheita mostrar safra concluída; nova safra usa Novo jogo confirmado.
Não introduzir preços reais, vendas, MQTT ou crescimento agronômico real.
Colheita usa um implemento rebocado ilustrativo neste protótipo.

> For agentic workers: use superpowers:executing-plans task-by-task.

## Plano

1. Testar migração e progressão com tentativas completas/incompletas, confirmação
   repetida, recuperação e falha de save. Criar catálogo reutilizável de operações.
2. Evoluir save para guardar operação seguinte e produtividade confirmada;
   migrar versões antigas preservando máquina, saldo, IDs e data.
3. Criar FieldWorkVisual com células/faixas, texturas e marcas geométricas locais,
   diferenciando intacto/revirado e estados de todas as operações. Sem assets externos.
4. Integrar catálogo ao motor de operação, HUD, implementos, eventos e telemetria.
   Um implemento próprio para cada operação, seleção obrigatória no galpão.
5. Testar ciclo de cada operação, cobertura visual nas duas direções, bloqueio
   após safra, interrupção e migração. Verificar aparência no Unity.
6. Aguardar aprovação funcional. Publicar ao avançar à etapa seguinte.

Arquivos principais: OperationCatalog.cs, FieldWorkVisual.cs,
PlaceholderOperation.cs, PlaceholderSlice.cs, SaveGameStore.cs, LocalOperationLog.cs
e validadores isolados em Editor. Alterações de cena serão feitas em execução
por componentes, preservando a cena salva e os placeholders existentes.

## Implementação e verificação

Status: implementada localmente; aguardando confirmação funcional do usuário.
Etapa 04 publicada em b5088a6, com serviços gratuitos.

O catálogo define nome, implemento, evento e estados do terreno. O mesmo motor
executa as cinco operações (30 segundos de trabalho, pausa para decisão em 45%).
Consumo/desgaste são ilustrativos e reutilizados nesta etapa. Não simulam
máquinas reais. A colhedora é rebocada apenas para validar o ciclo com o TR-01.

O campo tem 96 células em quatro passadas alternadas. Texturas locais geradas
em execução distinguem torrões, sulcos, sementes, brotos/adubo, plantas tratadas,
lavoura dourada e restolho. Pulverizador tem barra e névoa ilustrativas.
O crescimento/maturação entre etapas é instantâneo neste placeholder.

Save versão 3 mantém máquina, saldo, IDs e data e adiciona próxima operação e
produtividade. Saves v1/v2 começam no preparo, pois não continham progresso
do campo. A conclusão só avança ao confirmar o retorno ao galpão. Encerrar
antes disso preserva o checkpoint e registra a tentativa interrompida ou
concluída sem confirmação. O JSONL inclui tipo, implemento e índice da etapa,
também na recuperação. Uma confirmação repetida não avança duas etapas.
Após colheita, a saída fica bloqueada; Novo jogo reinicia a sequência.

Sete validadores passaram na compilação final: seleção, manutenção, save,
economia, motor VS01, progressão do campo e telemetria após retorno.
Incluem simulação das cinco operações, decisão acumulada, migração v1/v2,
interrupção, falha de confirmação/repetição, cobertura nas duas direções e
bloqueio depois da colheita. Testes usam arquivos temporários isolados.
Avisos de gravação nos testes de falha são intencionais.

## Roteiro de aprovação

1. Selecione TR-01 e acople o implemento indicado. Saia para o talhão.
2. Confira a transformação progressiva e resolva o evento agrícola.
3. Ao concluir, retorne ao galpão: a próxima operação e a data devem atualizar.
4. Repita plantio, fertilização, pulverização e colheita. Abasteça/repare
   gratuitamente quando necessário. Confira produtividade acumulada.
5. Reinicie Play no meio de uma operação: deve recuperar a etapa confirmada,
   combustível, desgaste e saldo do último galpão, com nova tentativa ao sair.
6. Depois da colheita, confirme o bloqueio e use Novo jogo se quiser recomeçar.

Melhorias visuais continuam pendentes: galpão com acabamento próprio, sprites
agrícolas e manobras contínuas nas cabeceiras. Esta etapa usa geometria simples.
Teste visual no Unity: executado ciclo completo das cinco operações, com
seleção obrigatória, implementos de cores/formas distintas, transformação
progressiva e retorno ao galpão. Preparo/plantio confirmados; fertilização
com decisão de continuar reduziu produtividade para 85%, preservada até
colheita. Pulverização interrompida em 45% recuperou 58 L/12,2%/85% no galpão
sem avançar; novo ID de tentativa, anterior marcado interrupted com índice 3.
Névoa e fileiras tratadas visíveis. Colheita transformou lavoura dourada em
restolho. Serviços gratuitos mantiveram o saldo preexistente de R$ 4.829,00.
O teste avança o save local real; use Novo jogo se quiser repetir a sequência.