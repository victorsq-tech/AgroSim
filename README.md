# AgroSim

Protótipo acadêmico de simulação de operações agrícolas em Unity 2D e C#.

## Abrir o projeto

1. Clone este repositório no computador.
2. No Unity Hub, use **Add project from disk** e selecione a pasta clonada.
3. Use **Unity 6000.6.4f1**, a versão registrada em `ProjectSettings/ProjectVersion.txt`.
4. Aguarde a restauração dos pacotes e a importação dos assets.
5. Abra `Assets/AgroSim/Scenes/Gameplay.unity` e pressione **Play**.

## Primeiro Vertical Slice

Galpão → selecionar TR-01 → acoplar grade → deslocar ao talhão → preparar
o solo → decidir sobre solo compactado → consultar resultado → voltar ao galpão.

Inclui placeholders para fazenda, trator e implemento; combustível,
desgaste e produtividade simulados; telemetria e resultado locais em JSONL.
Inclui uma tela exclusiva do galpão, abastecimento, reparo e deslocamento
pela estrada. Combustível e desgaste se mantêm entre operações na mesma
sessão. Os parâmetros são ilustrativos. Não inclui MQTT, economia nem
safra completa.

O histórico local fica em `Application.persistentDataPath/AgroSim` e não
é enviado ao GitHub. Ele não restaura o estado do jogo.

## Organização

- `Assets/AgroSim/Scripts`: lógica por área do jogo.
- `Assets/AgroSim/Scenes`: cena Gameplay e cópia da fundação inicial.
- `Assets/AgroSim/Prefabs`: placeholders de trator e implemento.
- `Assets/AgroSim/Data`: espaço para dados configuráveis.
- `Docs/VS01-status.md`: estado e limites do protótipo.

## Validação

No Editor, use **AgroSim → Validar ciclo VS01** para verificar a pausa do
evento, as duas consequências, a conclusão e o reinício. Teste também o
ciclo pela interface em Play. A versão inicial teve o ciclo completo e
o arquivo local conferidos no PC de Casa.

## Versionamento

Versione `Assets` com os respectivos arquivos `.meta`, `Packages`,
`ProjectSettings` e documentação. A pasta `Library` é recriada pelo Unity;
caches, builds, preferências locais e telemetria são ignorados.

As pastas vazias aparecerão ao serem preenchidas com conteúdo.
