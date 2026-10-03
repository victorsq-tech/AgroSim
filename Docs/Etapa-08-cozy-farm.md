# Etapa 08 — Ambiente Cozy Farm

O usuário escolheu e importou Cozy Farm. O pacote original permanece em Assets/Skyden_Games/Cozy Farm. Créditos e origem local registrados em Assets-de-terceiros.md.

Substituição da grama uniforme pelo sprite Ground Tiles_0, repetido no entorno do talhão e da estrada. Seis árvores nas laterais do campo e cinco nas bordas da estrada usam as variantes Tree_0 a Tree_3. As dimensões são limitadas a 1,1 unidade de largura e 1,3 de altura para deixar livres terreno e trajeto. Tree_4 é um toco largo e não participa dessa decoração.

Um asset de configuração em Assets/AgroSim/Resources mantém referências serializadas ao pacote. Ele é criado pelo Editor, e a execução lê essas referências sem depender de APIs do Editor. A grama usa um sprite de malha retangular gerado em memória a partir da textura original, sem modificar o arquivo ou importador do fornecedor; esse sprite é descartado ao fechar a cena.

Sem mudanças no save, economia ou lógica de operação. Visualizações do mapa e galpão continuam na apresentação da etapa anterior. Não foi adicionada câmera pixel perfect: o pacote usa ilustrações em alta resolução, e suas configurações de importação foram mantidas.

Verificação: referências da grama e dos prefabs disponíveis; dez validadores existentes aprovados após compilação. Talhão conferido visualmente no Editor. Estrada e limites das árvores conferidos visualmente após reiniciar Play. O teste de trajeto foi interrompido sem confirmar nova operação; o checkpoint de Colheita (5/5), 32,5 L, desgaste 10,8%, produtividade 85% e saldo R$ 5.000 foi preservado. Executável exportado não foi testado nesta etapa.