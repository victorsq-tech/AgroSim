# AgroSim

Protótipo acadêmico de simulação de operações agrícolas em Unity 2D e C#.

## Abrir o projeto

1. Clone este repositório no computador.
2. No Unity Hub, use **Add project from disk** e selecione a pasta clonada.
3. Use **Unity 6000.6.4f1**, a versão registrada em `ProjectSettings/ProjectVersion.txt`.
4. Aguarde a restauração dos pacotes e a importação dos assets.
5. Abra `Assets/AgroSim/Scenes/Gameplay.unity` e pressione **Play**.

## MVP atual

Menu inicial → mapa da fazenda → galpão → selecionar TR-01 e implemento →
acoplar e aguardar na porta → estrada → operação no talhão → decisão →
resultado → estrada de volta → confirmação e equipamentos guardados.

Cinco operações: preparo do solo, plantio, fertilização, pulverização e
colheita. O terreno muda conforme as passadas do trator e a etapa confirmada.
O galpão possui quatro vagas, com TR-01 na primeira e três vazias, além do
depósito de implementos. Oficina e abastecimento são acessíveis pelo mapa.

Combustível, desgaste, produtividade, saldo e próxima operação ficam no
save local. Serviços têm preço zero nesta fase do protótipo. Ao interromper
uma operação, o jogo volta ao último checkpoint confirmado e registra a
tentativa separadamente. Save e histórico JSONL ficam em
`Application.persistentDataPath/AgroSim` e não são enviados ao GitHub.

Cozy Farm, selecionado e importado pelo usuário, fornece grama, terra,
pisos, árvores e casas. Tratores e implementos usam placeholders locais.
Créditos e usos estão em [Docs/Assets-de-terceiros.md](Docs/Assets-de-terceiros.md).
Os parâmetros agrícolas são ilustrativos. MQTT e safra de vários dias
continuam pendentes.

## Organização

- `Assets/AgroSim/Scripts`: lógica por área do jogo.
- `Assets/AgroSim/Scenes`: cena Gameplay e cópia da fundação inicial.
- `Assets/AgroSim/Prefabs`: placeholders de trator e implemento.
- `Assets/AgroSim/Data`: espaço para dados configuráveis.
- `Docs/VS01-status.md`: histórico da fundação inicial.
- `Docs/Etapa-09-acabamento-mvp.md`: escopo e conferências da etapa visual atual.
- `Docs/Guia-das-pastas.md`: localização das telas, arte, configurações e save.
- `Assets/Skyden_Games/Cozy Farm`: pacote original preservado.

## Validação

No Editor, use **AgroSim → Validar ciclo VS01** para verificar a pausa do
evento, as duas consequências, a conclusão e o reinício. Teste também o
ciclo pela interface em Play. Os demais comandos AgroSim validam menus,
manobras, manutenção, economia, save, telemetria, enquadramento e progressão.

Para testar sem alterar seu progresso, use **AgroSim → Testes → Play com
save isolado**. **Retomar último save de teste** permite conferir recuperação
e persistência. Ao sair de Play, a seleção de teste é desativada; Play normal
usa o save original.

## Versionamento

Versione `Assets` com os respectivos arquivos `.meta`, `Packages`,
`ProjectSettings` e documentação. A pasta `Library` é recriada pelo Unity;
caches, builds, preferências locais e telemetria são ignorados.

As pastas vazias aparecerão ao serem preenchidas com conteúdo.
