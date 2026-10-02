# Etapa 02 — tela do galpão, manutenção e estrada

Status: aprovada pelo usuário em 02/10/2026; pronta para registro no GitHub.

Validação técnica: compilação e testes de preparação, operação e manutenção
passaram. Teste manual completou saída, evento, resultado, retorno, abastecimento
e reparo. Ao retornar, 80,5 L e 8,6% foram preservados; abastecimento alterou
somente combustível e reparo alterou somente desgaste.

## Escopo

- Tela exclusiva do galpão com áreas de equipamentos e manutenção.
- Seleção e acoplamento preservam as regras aprovadas na etapa 01.
- Abastecimento até 100 L e reparo até 0% de desgaste, gratuitos nesta versão.
- Combustível e desgaste acumulados entre operações na mesma sessão.
- Reserva mínima de 22,5 L e desgaste máximo de 91,4% para iniciar, cobrindo
  o maior custo de combustível e desgaste das decisões atuais.
- Saída e retorno por pontos da estrada, com orientação do trator.
- Consumo apresentado por operação, sem confundir com o histórico acumulado.

## Teste do usuário

1. Pressione Play: a tela do galpão ocupa a área do jogo.
2. Sem trator selecionado, a manutenção e a saída ficam bloqueadas.
3. Selecione TR-01 e acople a grade; saia para o talhão.
4. Confira a passagem pela estrada e as mudanças de direção.
5. Resolva o evento e conclua a operação.
6. Volte ao galpão e confira o combustível restante e o desgaste acumulado.
7. Selecione TR-01, abasteça e confirme tanque cheio sem apagar o desgaste.
8. Repare e confirme desgaste zerado sem alterar o combustível.
9. Repita a operação para verificar a continuidade do ciclo.

## Limites

Ainda usa formas simples e valores ilustrativos. A tela do galpão é uma
interface exclusiva dentro da cena Gameplay, sem exigir troca de cena.
Não há custos financeiros, tempo de oficina ou persistência do estado do
trator ao fechar o jogo. O histórico JSONL continua salvo localmente.
O deslocamento é visual, sem consumo adicional nesta etapa.

O usuário confirmou esta etapa e autorizou sua publicação no GitHub.
A etapa seguinte será planejada e revisada antes de começar a implementação.
