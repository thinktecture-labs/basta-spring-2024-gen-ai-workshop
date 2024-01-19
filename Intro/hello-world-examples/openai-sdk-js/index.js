const fs = require('fs');
const { OpenAIClient, OpenAIKeyCredential } = require('@azure/openai');

async function main() {
    // Read api key
    const apiKey = fs.readFileSync('openaiapikey.txt', 'utf8');
    if (!apiKey) {
        throw new Error('apiKey is not set');
    }

    // create client
    const client = new OpenAIClient(new OpenAIKeyCredential(apiKey));
    const deploymentId = "gpt-3.5-turbo";

    const messages = [
        { role: "system", content: "You are a helpful assistant. You will talk like a pirate." },
        { role: "user", content: "Can you help me?" },
        { role: "assistant", content: "Arrrr! Of course, me hearty! What can I do for ye?" },
        { role: "user", content: "What's the best way to train a parrot on using an LLM?" },
      ];

    console.log(`Messages: ${messages.map((m) => m.content).join("\n")}`);

    const events = await client.streamChatCompletions(deploymentId, messages, { maxTokens: 128 });
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
