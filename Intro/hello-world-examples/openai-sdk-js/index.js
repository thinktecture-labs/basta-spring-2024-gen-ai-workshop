const fs = require('fs');
const { OpenAIClient, OpenAIKeyCredential } = require('@azure/openai');

async function main() {
    // Read api key
    const apiKey = process.env.OPENAI_API_KEY;
    if (!apiKey) {
        throw new Error('apiKey is not set');
    }

    // create client
    const client = new OpenAIClient(new OpenAIKeyCredential(apiKey));
    const deploymentId = "gpt-3.5-turbo";

    const messages = [
        { role: "system", content: `Du bist ein freundlicher, fröhlicher und hilfsbereiter Assistent.
            Wir sind in Frankfurt auf einer Konferenz für .NET Entwickler namens BASTA!, in einem Workshop zu Generative AI im Business-Umfeld.
            Es ist Rosenmontag, und einer der Speaker ist Rheinländer und Karnevalist.
            Du antwortest deswegen im Stil einer lustigen Büttenrede, mit sich reimenden Vierzeilern.

            Beispiel:

            Erklingt auf einer Konferenz,
            ganz plötzlich eine Flatulenz,
            aus einem gut gepflegten Po,
            guckt jeder weg, ist heimlich froh,

            dass es nicht sein Hintern war,
            dem das Missgeschick geschah.
            Das betrübt mich wirklich sehr,
            denn so viel Scham, die wiegt so schwer.

            Drum forder ich für Flatulenzen,
            ein Ausbleiben von Konsequenzen.
            Wenn das Gesäß sich lüften muss,
            dann ist jetzt mit dem Schämen Schluss.
            ` },
        { role: "user", content: `Bitte begrüße unsere Teilnehmer.` },
      ];

    console.log(`Messages: ${messages.map((m) => m.content).join("\n")}`);

    const events = await client.streamChatCompletions(deploymentId, messages, { temperature: 0.9, });
    console.log("Chatbot:");
    for await (const event of events) {
        for (const choice of event.choices) {
            const delta = choice.delta?.content;
            if (delta !== undefined) {
                process.stdout.write(delta);
            }
        }
    }
}

main().catch((err) => {
    console.error("The sample encountered an error:", err);
});
