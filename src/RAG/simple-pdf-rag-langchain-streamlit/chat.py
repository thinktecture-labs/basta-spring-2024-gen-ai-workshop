import os
import tempfile

import streamlit as st
from langchain.callbacks.base import BaseCallbackHandler
from langchain.chains import ConversationalRetrievalChain
from langchain.memory import ConversationBufferMemory
from langchain.text_splitter import RecursiveCharacterTextSplitter

#from langchain_openai import ChatOpenAI, OpenAIEmbeddings
from langchain_community.document_loaders import PyPDFLoader
from langchain_community.embeddings import HuggingFaceEmbeddings
from langchain_community.vectorstores import FAISS
from langchain_openai import ChatOpenAI

st.set_page_config(page_title="Chat with PDF", page_icon="🦜")
st.title("🦜 Chat with PDF")

@st.cache_resource(ttl="1h")
def configure_retriever(uploaded_files):
    docs = []
    temp_dir = tempfile.TemporaryDirectory()
    for file in uploaded_files:
        temp_filepath = os.path.join(temp_dir.name, file.name)
        with open(temp_filepath, "wb") as f:
            f.write(file.getvalue())
        loader = PyPDFLoader(temp_filepath)
        docs.extend(loader.load())

    text_splitter = RecursiveCharacterTextSplitter(chunk_size=10000, chunk_overlap=20)
    splits = text_splitter.split_documents(docs)

    embeddings = HuggingFaceEmbeddings(model_name="intfloat/multilingual-e5-large")
    vectordb = FAISS.from_documents(splits, embedding=embeddings)

    retriever = vectordb.as_retriever(search_kwargs={"k": 3})

    return retriever

class StreamHandler(BaseCallbackHandler):
    def __init__(self, container: st.delta_generator.DeltaGenerator, initial_text: str = ""):
        self.container = container
        self.text = initial_text
        self.run_id_ignore_token = None

    def on_llm_start(self, serialized: dict, prompts: list, **kwargs):
        # Workaround to prevent showing the rephrased question as output
        if prompts[0].startswith("Human"):
            self.run_id_ignore_token = kwargs.get("run_id")

    def on_llm_new_token(self, token: str, **kwargs) -> None:
        if self.run_id_ignore_token == kwargs.get("run_id", False):
            return
        self.text += token
        self.container.markdown(self.text)

class PrintRetrievalHandler(BaseCallbackHandler):
    def __init__(self, container):
        self.status = container.status("**Context Retrieval**")

    def on_retriever_start(self, serialized: dict, query: str, **kwargs):
        self.status.write(f"**Question:** {query}")
        self.status.update(label=f"**Context Retrieval:** {query}")

    def on_retriever_end(self, documents, **kwargs):
        for idx, doc in enumerate(documents):
            source = os.path.basename(doc.metadata["source"])
            self.status.write(f"**Document {idx} from {source}**")
            self.status.markdown(doc.page_content)
        self.status.update(state="complete")

uploaded_files = st.sidebar.file_uploader(
    label="Upload PDF files", type=["pdf"], accept_multiple_files=True)

if not uploaded_files:
    st.info("Please upload PDF documents to continue.")
    st.stop()

retriever = configure_retriever(uploaded_files)

memory = ConversationBufferMemory(memory_key="chat_history", return_messages=True, output_key='answer')

# Here you can change the base URL to e.g. point to a non-OpenAI LLM which has OpenAI API compatibility
llm = ChatOpenAI(#openai_api_base="http://localhost:8082/v1", 
                 temperature=0.0,
                 model="gpt-4",
                 streaming=True)

qa_chain = ConversationalRetrievalChain.from_llm(
    llm, chain_type='stuff', retriever=retriever, memory=memory)

if "messages" not in st.session_state:
    st.session_state.messages = []

for message in st.session_state.messages:
    with st.chat_message(message["role"]):
        st.markdown(message["content"])

if prompt := st.chat_input("Ask your question:"):
    prompt = prompt + "\nGive the result in three compact paragraphs."
    
    st.session_state.messages.append({"role": "user", "content": prompt})
    with st.chat_message("user"):
        st.markdown(prompt)

    retrieval_handler = PrintRetrievalHandler(st.container())
    stream_handler = StreamHandler(st.empty())
    
    result = qa_chain({"question": prompt, "chat_history": [(message["role"], message["content"]) for message in st.session_state.messages]}, callbacks=[retrieval_handler, stream_handler])
    print("\n\nPROMPT:\n"+ prompt)

    st.session_state.messages.append({"role": "assistant", "content": result["answer"]})
