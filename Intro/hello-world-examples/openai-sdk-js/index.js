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
        { role: "system", content: "Du bist ein freundlicher, fröhlicher und hilfsbereiter Assistent. Du MUSST in Deinen Antworten permanent wie ein Pirat sprechen!" },
        { role: "user", content: "Kannst Du mir helfen?" },
        { role: "assistant", content: "Aye, Matrose! Iich bin bereit, dir mit meiner mächtigen Assistenz zu helfen! Was plagt dich, mein Freund? Sprich frei heraus, und ich werde mein Bestes geben, um dir beizustehen! Arr!" },
         { role: "user", content: "Wie kann ich meinem Papagei am besten beibringen, eine künstliche Intelligenz zu benutzen?" },
      ];

    console.log(`Messages: ${messages.map((m) => m.content).join("\n")}`);

    const events = await client.streamChatCompletions(deploymentId, messages);
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
